function Compile(input) {
    let result = myTranspile(input, { module: ts.ModuleKind.CommonJS, diagnostics: true });
    return result;
}

function myTranspile(input, compilerOptions, fileName) {
    diagnostics = [];
    var options = compilerOptions ? ts.clone(compilerOptions) : getDefaultCompilerOptions();
    options.separateCompilation = false;
    // Filename can be non-ts file.
    options.allowNonTsExtensions = true;
    options.diagnostics = true;
    

    // Parse
    var inputFileName = fileName || 'module.ts';
    var sourceFile = ts.createSourceFile(inputFileName, input, options.target);
    var libfile = ts.createSourceFile('lib.d.ts', libsource, options.target);
    // Store syntactic diagnostics
    if (diagnostics && sourceFile.parseDiagnostics) {
        diagnostics.push.apply(diagnostics, sourceFile.parseDiagnostics);
    }
    // Output
    var outputText;


    // Create a compilerHost object to allow the compiler to read and write files
    var compilerHost = {
        getSourceFile: function (fileName, target) {

            if (fileName === 'lib.d.ts')
                return libfile;

            return fileName === inputFileName ? sourceFile : undefined;
        },
        writeFile: function (name, text, writeByteOrderMark) {
            ts.Debug.assert(outputText === undefined, 'Unexpected multiple outputs for the file: ' + name);
            outputText = text;
        },
        getDefaultLibFileName: function () { return 'lib.d.ts'; },
        useCaseSensitiveFileNames: function () { return false; },
        getCanonicalFileName: function (fileName) { return fileName; },
        getCurrentDirectory: function () { return ''; },
        getNewLine: function () { return (ts.sys && ts.sys.newLine) || '\r\n'; }
    };
    var program = ts.createProgram([inputFileName], options, compilerHost);
    //if (diagnostics) {
    //    diagnostics.push.apply(diagnostics, program.getGlobalDiagnostics());
    //}
    // Emit
    program.emit();


    var declaration = program.getDeclarationDiagnostics(sourceFile);
    var global = program.getGlobalDiagnostics();
    var semantic = program.getSemanticDiagnostics(sourceFile);
    var syntactic = program.getSyntacticDiagnostics(sourceFile);

    diagnostics = declaration.concat(global).concat(semantic).concat(syntactic);

    var erg = new Object();
    erg.output = outputText;
    erg.diagnostics = diagnostics;

    ts.Debug.assert(outputText !== undefined, 'Output generation failed');
    return erg;
}