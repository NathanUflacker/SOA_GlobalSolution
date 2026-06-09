(function () {
    const translations = new Map([
        ["Alerts", "Alertas"],
        ["Auth", "Autenticação"],
        ["Dashboard", "Painel"],
        ["Satellites", "Satélites"],
        ["SpaceDebris", "Detritos Espaciais"],
        ["Parameters", "Parâmetros"],
        ["No parameters", "Sem parâmetros"],
        ["Responses", "Respostas"],
        ["Server responses", "Respostas do servidor"],
        ["Code", "Código"],
        ["Description", "Descrição"],
        ["Links", "Links"],
        ["No links", "Sem links"],
        ["Try it out", "Testar"],
        ["Cancel", "Cancelar"],
        ["Execute", "Executar"],
        ["Clear", "Limpar"],
        ["Download", "Baixar"],
        ["Request body", "Corpo da requisição"],
        ["Response body", "Corpo da resposta"],
        ["Response headers", "Cabeçalhos da resposta"],
        ["Response content type", "Tipo de conteúdo da resposta"],
        ["Example Value", "Valor de exemplo"],
        ["Schema", "Esquema"],
        ["Media type", "Tipo de mídia"],
        ["Name", "Nome"],
        ["Located in", "Localizado em"],
        ["Required", "Obrigatório"],
        ["Type", "Tipo"],
        ["Deprecated", "Obsoleto"],
        ["Available authorizations", "Autorizações disponíveis"],
        ["Authorize", "Autorizar"],
        ["Close", "Fechar"],
        ["Logout", "Sair"],
        ["Scopes", "Escopos"]
    ]);

    function translateNode(node) {
        if (node.nodeType !== Node.TEXT_NODE) return;

        const original = node.textContent;
        const trimmed = original.trim();
        if (!translations.has(trimmed)) return;

        node.textContent = original.replace(trimmed, translations.get(trimmed));
    }

    function translateAttributes(element) {
        if (!(element instanceof HTMLElement)) return;

        for (const attr of ["title", "aria-label", "placeholder", "value"]) {
            const value = element.getAttribute(attr);
            if (value && translations.has(value.trim())) {
                element.setAttribute(attr, translations.get(value.trim()));
            }
        }
    }

    function translateSwaggerUi() {
        const root = document.querySelector("#swagger-ui");
        if (!root) return;

        const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
        while (walker.nextNode()) {
            translateNode(walker.currentNode);
        }

        root.querySelectorAll("*").forEach(translateAttributes);
    }

    const observer = new MutationObserver(translateSwaggerUi);

    function start() {
        translateSwaggerUi();
        observer.observe(document.body, { childList: true, subtree: true, characterData: true });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", start);
    } else {
        start();
    }
})();
