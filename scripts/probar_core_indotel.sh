#!/usr/bin/env bash
set -u

BASE="${BASE:-http://localhost:5085}"
ADMIN_CORREO="${ADMIN_CORREO:-admin@indotel.test}"
ADMIN_CLAVE="${ADMIN_CLAVE:-Admin123*}"

echo "===================================="
echo "PRUEBA CORE INDOTEL"
echo "===================================="

echo ""
echo "1) Login admin..."
TOKEN=$(curl -s -X POST "$BASE/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"correo\":\"$ADMIN_CORREO\",\"password\":\"$ADMIN_CLAVE\"}" \
  | python3 -c "import sys,json; print(json.load(sys.stdin)['token'])")

if [ -z "$TOKEN" ]; then
  echo "ERROR: No se pudo obtener token"
  exit 1
fi

echo "OK: token guardado"

probar_get_200 () {
  NOMBRE="$1"
  URL="$2"

  STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" "$URL" \
    -H "Authorization: Bearer $TOKEN")

  if [ "$STATUS" = "200" ]; then
    echo "OK: $NOMBRE -> $STATUS"
  else
    echo "ERROR: $NOMBRE -> $STATUS"
    cat /tmp/resp_indotel.json
    echo ""
  fi
}

echo ""
echo "2) Probando endpoints base..."
probar_get_200 "Servicios alias" "$BASE/api/servicios"
probar_get_200 "Prestadoras alias" "$BASE/api/prestadoras"
probar_get_200 "Usuarios" "$BASE/api/usuarios"
probar_get_200 "Ciudadanos" "$BASE/api/ciudadanos"
probar_get_200 "Reclamaciones" "$BASE/api/reclamaciones"
probar_get_200 "Reporte resumen" "$BASE/api/reportes/resumen"

echo ""
echo "3) Crear reclamacion para flujo de estados..."

RECLAMACION_JSON=$(curl -s -X POST "$BASE/api/reclamaciones" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "ciudadanoId": 1,
    "prestadoraId": 1,
    "servicioTelecomId": 1,
    "titulo": "Prueba automatica de flujo",
    "descripcion": "Reclamacion creada por script para probar estados y documentos."
  }')

echo "$RECLAMACION_JSON" > /tmp/reclamacion_indotel.json

RECLAMACION_ID=$(python3 -c "import json; print(json.load(open('/tmp/reclamacion_indotel.json'))['id'])")
EXPEDIENTE=$(python3 -c "import json; print(json.load(open('/tmp/reclamacion_indotel.json'))['numeroExpediente'])")

echo "OK: reclamacion creada"
echo "ID=$RECLAMACION_ID"
echo "EXPEDIENTE=$EXPEDIENTE"

echo ""
echo "4) Probar salto invalido RECIBIDA -> CERRADA, debe dar 409..."

STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" -X PATCH "$BASE/api/reclamaciones/$RECLAMACION_ID/estado" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "estadoNuevo": "CERRADA",
    "comentario": "Intento invalido desde RECIBIDA."
  }')

if [ "$STATUS" = "409" ]; then
  echo "OK: salto invalido rechazado con 409"
else
  echo "ERROR: salto invalido debio dar 409 y dio $STATUS"
  cat /tmp/resp_indotel.json
  echo ""
fi

cambiar_estado () {
  ESTADO="$1"
  COMENTARIO="$2"

  STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" -X PATCH "$BASE/api/reclamaciones/$RECLAMACION_ID/estado" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{
      \"estadoNuevo\": \"$ESTADO\",
      \"comentario\": \"$COMENTARIO\"
    }")

  if [ "$STATUS" = "200" ]; then
    echo "OK: cambio a $ESTADO -> $STATUS"
  else
    echo "ERROR: cambio a $ESTADO -> $STATUS"
    cat /tmp/resp_indotel.json
    echo ""
  fi
}

echo ""
echo "5) Flujo correcto de estados..."
cambiar_estado "VALIDADA" "Reclamacion validada."
cambiar_estado "ENVIADA_A_PRESTADORA" "Caso enviado a la prestadora."

echo ""
echo "6) Respuesta prestadora..."

STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" -X POST "$BASE/api/reclamaciones/$RECLAMACION_ID/respuesta-prestadora" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "prestadoraId": 1,
    "respuesta": "La prestadora informa que reviso el caso y aplico correccion.",
    "documentoSoporte": "respuesta-demo.pdf"
  }')

if [ "$STATUS" = "200" ]; then
  echo "OK: respuesta prestadora -> $STATUS"
else
  echo "ERROR: respuesta prestadora -> $STATUS"
  cat /tmp/resp_indotel.json
  echo ""
fi

cambiar_estado "EN_REVISION" "INDOTEL revisa la respuesta de la prestadora."
cambiar_estado "RESUELTA" "Caso resuelto satisfactoriamente."
cambiar_estado "CERRADA" "Caso cerrado luego de resolucion."

echo ""
echo "7) Confirmar que CERRADA no vuelve atras, debe dar 409..."

STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" -X PATCH "$BASE/api/reclamaciones/$RECLAMACION_ID/estado" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "estadoNuevo": "VALIDADA",
    "comentario": "Intento invalido despues de cerrar."
  }')

if [ "$STATUS" = "409" ]; then
  echo "OK: reclamacion cerrada no permite volver atras -> 409"
else
  echo "ERROR: debio dar 409 y dio $STATUS"
  cat /tmp/resp_indotel.json
  echo ""
fi

echo ""
echo "8) Historial de la reclamacion..."

STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" "$BASE/api/reclamaciones/$RECLAMACION_ID/historial" \
  -H "Authorization: Bearer $TOKEN")

if [ "$STATUS" = "200" ]; then
  echo "OK: historial -> $STATUS"
  cat /tmp/resp_indotel.json
  echo ""
else
  echo "ERROR: historial -> $STATUS"
  cat /tmp/resp_indotel.json
  echo ""
fi

echo ""
echo "9) Probar documentos en reclamacion cerrada, debe dar 409..."

echo "Documento de prueba INDOTEL" > /tmp/evidencia-prueba.pdf

STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" -X POST "$BASE/api/reclamaciones/$RECLAMACION_ID/documentos" \
  -H "Authorization: Bearer $TOKEN" \
  -F "archivo=@/tmp/evidencia-prueba.pdf")

if [ "$STATUS" = "409" ]; then
  echo "OK: no permite subir documento a caso cerrado -> 409"
else
  echo "ERROR: documento en caso cerrado debio dar 409 y dio $STATUS"
  cat /tmp/resp_indotel.json
  echo ""
fi

echo ""
echo "10) Crear reclamacion abierta para probar documentos..."

DOC_RECLAMACION_JSON=$(curl -s -X POST "$BASE/api/reclamaciones" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "ciudadanoId": 1,
    "prestadoraId": 1,
    "servicioTelecomId": 1,
    "titulo": "Prueba automatica de documentos",
    "descripcion": "Reclamacion abierta para probar carga de evidencias."
  }')

echo "$DOC_RECLAMACION_JSON" > /tmp/doc_reclamacion_indotel.json
DOC_RECLAMACION_ID=$(python3 -c "import json; print(json.load(open('/tmp/doc_reclamacion_indotel.json'))['id'])")

echo "OK: reclamacion abierta creada"
echo "DOC_RECLAMACION_ID=$DOC_RECLAMACION_ID"

echo ""
echo "11) Subir documento a reclamacion abierta..."

STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" -X POST "$BASE/api/reclamaciones/$DOC_RECLAMACION_ID/documentos" \
  -H "Authorization: Bearer $TOKEN" \
  -F "archivo=@/tmp/evidencia-prueba.pdf")

if [ "$STATUS" = "201" ]; then
  echo "OK: documento subido -> 201"
  cat /tmp/resp_indotel.json
  echo ""
else
  echo "ERROR: subir documento -> $STATUS"
  cat /tmp/resp_indotel.json
  echo ""
fi

echo ""
echo "12) Listar documentos..."

STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" "$BASE/api/reclamaciones/$DOC_RECLAMACION_ID/documentos" \
  -H "Authorization: Bearer $TOKEN")

if [ "$STATUS" = "200" ]; then
  echo "OK: listar documentos -> 200"
  cat /tmp/resp_indotel.json
  echo ""
else
  echo "ERROR: listar documentos -> $STATUS"
  cat /tmp/resp_indotel.json
  echo ""
fi

echo ""
echo "13) Probar consulta por expediente..."

STATUS=$(curl -s -o /tmp/resp_indotel.json -w "%{http_code}" "$BASE/api/reclamaciones/expediente/$EXPEDIENTE" \
  -H "Authorization: Bearer $TOKEN")

if [ "$STATUS" = "200" ]; then
  echo "OK: consulta por expediente -> 200"
else
  echo "ERROR: consulta por expediente -> $STATUS"
  cat /tmp/resp_indotel.json
  echo ""
fi

echo ""
echo "===================================="
echo "PRUEBA TERMINADA"
echo "Reclamacion flujo ID: $RECLAMACION_ID"
echo "Expediente: $EXPEDIENTE"
echo "Reclamacion documentos ID: $DOC_RECLAMACION_ID"
echo "===================================="
