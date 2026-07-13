document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("form").forEach(form => {
        form.addEventListener("submit", event => {
            if (form.dataset.allowMultipleSubmit === "true") {
                return;
            }

            if (typeof form.checkValidity === "function" && !form.checkValidity()) {
                return;
            }

            const submitters = form.querySelectorAll(
                "button[type='submit'], input[type='submit']");

            submitters.forEach(button => {
                button.disabled = true;
                button.setAttribute("aria-disabled", "true");

                if (button.tagName === "BUTTON" && !button.dataset.originalText) {
                    button.dataset.originalText = button.textContent ?? "";
                    button.textContent = "Procesando...";
                }
            });

            form.setAttribute("aria-busy", "true");

            // El navegador continuará con el envío normal. No se reenvían solicitudes.
            if (event.defaultPrevented) {
                submitters.forEach(button => {
                    button.disabled = false;
                    button.removeAttribute("aria-disabled");
                });
                form.removeAttribute("aria-busy");
            }
        });
    });
});
