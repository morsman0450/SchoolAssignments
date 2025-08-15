// wwwroot/js/monaco-init.js
window.initMonaco = (elementId, initialCode, language) => {
    require.config({ paths: { 'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.45.0/min/vs' } });
    require(['vs/editor/editor.main'], function () {
        var editor = monaco.editor.create(document.getElementById(elementId), {
            value: initialCode,
            language: language,
            theme: "vs-dark",
            automaticLayout: true
        });

        editor.onDidChangeModelContent(() => {
            DotNet.invokeMethodAsync('SchoolAssignments', 'UpdateCode', editor.getValue());
        });
    });
};
