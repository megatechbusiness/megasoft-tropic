
/**
 * Evaluate JavaScript text in outer function such that all DHMTLIB/CRViewer resources are available in scope
 */
bobj.evalScript = function(scriptText) {
	eval(scriptText);
}
})();