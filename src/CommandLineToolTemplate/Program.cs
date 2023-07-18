using CommandLineTools;

return await new CommandLineToolTemplateTool().RunAsync(args); // top-level execution
// return await new CommandLineToolTemplateSubCommandTool().RunAsync(args); // sub-command execution