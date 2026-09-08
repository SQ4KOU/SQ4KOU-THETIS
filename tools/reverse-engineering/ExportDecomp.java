// Ghidra headless post-script. Exports decompiler C-like pseudocode for every
// non-external function, split into bounded text chunks, plus an exact index.

import ghidra.app.decompiler.DecompInterface;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.FunctionIterator;

import java.io.BufferedWriter;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;

public class ExportDecomp extends GhidraScript {
    private static final long CHUNK_LIMIT_CHARS = 4L * 1024L * 1024L;

    private static String csv(String value) {
        if (value == null) value = "";
        return "\"" + value.replace("\"", "\"\"").replace("\r", " ").replace("\n", " ") + "\"";
    }

    private static String safe(String value) {
        if (value == null || value.isEmpty()) return "program";
        return value.replaceAll("[^A-Za-z0-9._-]+", "_");
    }

    @Override
    public void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length < 1) {
            throw new IllegalArgumentException("ExportDecomp.java requires output directory argument");
        }

        Path outDir = Paths.get(args[0]);
        Files.createDirectories(outDir);
        String programName = safe(currentProgram.getName());
        Path indexPath = outDir.resolve("functions.csv");
        Path statsPath = outDir.resolve("decompile-stats.txt");

        int chunkNo = 1;
        long chunkChars = 0;
        Path chunkPath = outDir.resolve(String.format("%s.decompiled.part-%04d.c", programName, chunkNo));
        BufferedWriter chunk = Files.newBufferedWriter(chunkPath, StandardCharsets.UTF_8,
                StandardOpenOption.CREATE, StandardOpenOption.TRUNCATE_EXISTING);
        BufferedWriter index = Files.newBufferedWriter(indexPath, StandardCharsets.UTF_8,
                StandardOpenOption.CREATE, StandardOpenOption.TRUNCATE_EXISTING);
        index.write("entry,name,signature,status,chunk\n");

        DecompInterface decompiler = new DecompInterface();
        decompiler.toggleCCode(true);
        decompiler.toggleSyntaxTree(false);
        decompiler.setSimplificationStyle("decompile");
        if (!decompiler.openProgram(currentProgram)) {
            chunk.close();
            index.close();
            decompiler.dispose();
            throw new RuntimeException("Ghidra decompiler could not open program: " + currentProgram.getName());
        }

        int total = 0;
        int ok = 0;
        int failed = 0;
        int external = 0;
        FunctionIterator functions = currentProgram.getFunctionManager().getFunctions(true);
        while (functions.hasNext() && !monitor.isCancelled()) {
            Function f = functions.next();
            total++;
            if (f.isExternal()) {
                external++;
                continue;
            }

            String status = "OK";
            String body = null;
            String lastError = "";
            Throwable lastException = null;
            String[] styles = new String[] { "decompile", "normalize", "register" };
            for (String style : styles) {
                if (body != null || monitor.isCancelled()) break;
                try {
                    decompiler.flushCache();
                    decompiler.setSimplificationStyle(style);
                    DecompileResults result = decompiler.decompileFunction(f, 600, monitor);
                    if (result.decompileCompleted() && result.getDecompiledFunction() != null) {
                        body = result.getDecompiledFunction().getC();
                        status = style.equals("decompile") ? "OK" : "OK_RETRY_" + style.toUpperCase();
                    } else {
                        lastError = result.getErrorMessage();
                    }
                } catch (Throwable ex) {
                    lastException = ex;
                    lastError = ex.getClass().getName() + ": " + ex.getMessage();
                }
            }
            decompiler.setSimplificationStyle("decompile");
            if (body != null) {
                ok++;
            } else {
                status = (lastException == null ? "FAILED: " : "EXCEPTION: ") + lastError;
                body = "/* DECOMPILATION FAILED AFTER ALL STRICT RETRIES: " + lastError + " */\n";
                failed++;
                println("STRICT_DECOMP_FAILURE entry=" + f.getEntryPoint() +
                        " name=" + f.getName() + " error=" + lastError);
            }

            String header = "\n/* ========================================================================\n" +
                    "   ENTRY: " + f.getEntryPoint() + "\n" +
                    "   NAME : " + f.getName() + "\n" +
                    "   SIG  : " + f.getSignature().toString() + "\n" +
                    "   ======================================================================== */\n";
            String block = header + body + "\n";

            if (chunkChars > 0 && chunkChars + block.length() > CHUNK_LIMIT_CHARS) {
                chunk.close();
                chunkNo++;
                chunkChars = 0;
                chunkPath = outDir.resolve(String.format("%s.decompiled.part-%04d.c", programName, chunkNo));
                chunk = Files.newBufferedWriter(chunkPath, StandardCharsets.UTF_8,
                        StandardOpenOption.CREATE, StandardOpenOption.TRUNCATE_EXISTING);
            }

            chunk.write(block);
            chunkChars += block.length();
            index.write(csv(f.getEntryPoint().toString()) + "," +
                    csv(f.getName()) + "," +
                    csv(f.getSignature().toString()) + "," +
                    csv(status) + "," +
                    csv(chunkPath.getFileName().toString()) + "\n");
        }

        chunk.close();
        index.close();
        decompiler.dispose();

        try (BufferedWriter stats = Files.newBufferedWriter(statsPath, StandardCharsets.UTF_8,
                StandardOpenOption.CREATE, StandardOpenOption.TRUNCATE_EXISTING)) {
            stats.write("program=" + currentProgram.getName() + "\n");
            stats.write("language=" + currentProgram.getLanguageID() + "\n");
            stats.write("compiler=" + currentProgram.getCompilerSpec().getCompilerSpecID() + "\n");
            stats.write("functions_total=" + total + "\n");
            stats.write("functions_external_skipped=" + external + "\n");
            stats.write("functions_decompiled_ok=" + ok + "\n");
            stats.write("functions_failed=" + failed + "\n");
            stats.write("chunks=" + chunkNo + "\n");
            stats.write("cancelled=" + monitor.isCancelled() + "\n");
        }
    }
}
