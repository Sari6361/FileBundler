using System.CommandLine;
using System.IO;
using System.Net.Http.Headers;
using System.Security;
//global
bool isNote;
bool RemoveLines;
List<string> selectedLanguage;
string authorName;

//output option 
var bundleOption = new Option<FileInfo>(
    new string[] { "--output", "--o", "--out" }, () => new FileInfo(Directory.GetCurrentDirectory() + "\\files.txt"), "File path and name");

//languages option
Dictionary<string, string[]> languages = new Dictionary<string, string[]>();
#region makeDictionary
languages.Add("C#", new string[] { "cs" });
languages.Add("C++", new string[] { "cpp", "h" });
languages.Add("C", new string[] { "c", "h" });
languages.Add("Html", new string[] { "html", "js", "css" });
languages.Add("Java", new string[] { "java" });
languages.Add("React", new string[] { "scss", "ts", "js", "html" });
languages.Add("Angular", new string[] { "css", "scss", "js", "ts", "html" });
languages.Add("Python", new string[] { "py" });
languages.Add("Sql", new string[] { "sql" });
languages.Add("All", new string[] { "c", "h", "cpp", "cs", "js", "html", "ts", "py", "css", "sql", "scss" });
#endregion
var languagesOption = new Option<List<string>>(new string[] { "--language", "--l", "--lang" }, "An option whose argument is the languages parse.").FromAmong(languages.Keys.ToArray());
languagesOption.IsRequired = true;
languagesOption.AllowMultipleArgumentsPerToken = true;

//note option
var noteOption = new Option<bool>(new string[] { "--n", "--note" }, () => false, "An option whose argument defined if to put the contens whith note or not.");

//remove empty lines option
string[] aliases = new string[] { "--remove-empty-lines", "--r", "--remove-lines" };
var removeEmptyLinesOption = new Option<bool>(aliases, () => false, "An option whose argument is removed the empty lines from the file");

//sort option
var sortOption = new Option<string>(new string[] { "--sort", "--s" }, () => "AlpaBeticOrder", "An option which discribe how to sort the wriiten files").FromAmong("AlpaBeticOrder", "TypeOrder");

//autor option
var authorOption = new Option<string>(new string[] { "--a", "--author" }, () => null, "An option whose argument is the author name.");

//comand
var bundleCommand = new Command("bundle", "Bundle code file to a signal file");
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(authorOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(languagesOption);
bundleCommand.SetHandler((output, note, r, l, author) =>
{
    isNote = note;
    RemoveLines = r;
    selectedLanguage = l;
    authorName = author;
    try
    {
        try
        {
            FileStream outputFile = new FileStream(output.FullName, FileMode.Create, FileAccess.Write);
            Console.WriteLine("Bundle Command. File was created Start.");
            ReadAndWrite(outputFile, output.FullName);
            Console.WriteLine("Bundle Command. File was created at " + output.FullName);
        }
        catch (NullReferenceException) { Console.WriteLine("Error: not set "); }
    }
    catch (IOException)
    { Console.WriteLine("ERROR: Path is NOT valid!!"); }
}, bundleOption, noteOption, removeEmptyLinesOption, languagesOption, authorOption);

var createRspCommand = new Command("create-rsp", "Create An response file");

var rootCommand = new RootCommand("Root Command for File Bundler CLI.");

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);

await rootCommand.InvokeAsync(args);


void ReadAndWrite(FileStream outputFile, string path)
{
    try
    {
        string currentPath = Directory.GetCurrentDirectory();
        List<string> files = new List<string>();
        List<string> exstention = new List<string>();
        foreach (var item in selectedLanguage)
            exstention.AddRange(languages.GetValueOrDefault(item).ToArray());
        for (int i = 0; i < exstention?.Count; i++)
        {
            string[] s = Directory.GetFiles(currentPath, "*." + exstention?[i], SearchOption.AllDirectories);
            files.AddRange(s.ToList<string>().Where(f => !f.Contains("bin")).Where(f => !f.Contains("Debug")));
        }
        if (sortOption?.ToString() == "AlpaBeticOrder")
            files.OrderBy(a => a);
        else
            files.OrderBy(a => a.Split('.')[1]);
        Write(files, outputFile, currentPath);
        outputFile.Close();

    }
    catch (IOException) { Console.WriteLine("ERROR: Path is NOT valid!!"); };

}
void Write(List<string> files, FileStream outputFile, string currentPath)
{
    StreamWriter outputWriter = new StreamWriter(outputFile);
    if (authorName != null)
        outputWriter.WriteLine("//Author: " + authorName);
    files.ForEach(file =>
    {
        if (isNote)
            outputWriter.WriteLine("// File Name: " + Path.GetFileName(file) + ".");
        StreamReader reader = new StreamReader(file);
        if (RemoveLines)
            RemoveEmptyLines(outputWriter, reader);
        else
            outputWriter.Write(reader.ReadToEnd());
        outputWriter.Write("--------------------------------------------------------------------------------------------------------------------------------------------------------------");
        reader.Close();
    });
    outputWriter.Close();
}
void RemoveEmptyLines(StreamWriter streamWriter, StreamReader reader)
{
    string s = "";
    string line;
    streamWriter.WriteLine("//File Name: ");
    while ((line = reader.ReadLine()) != null)
    {
        if (line.Length > 1)
            s += line + "\n";
        line = null;
    }
    streamWriter.Write(s);
}

void create_rsp()
{
    List<string> lang = new List<string>();
    string option, author, sort = "AlpaBeticOrder", s;
    bool removeLines = false, note = false;

    FileStream response = new FileStream("Response.rsp", FileMode.Create, FileAccess.Write);

    Console.WriteLine("Enter the name of the exported bundle file. \n" +
        "You can only type in a file name and then the file will be saved in the location where the user ran the command.\n" +
        "(Not Requiered!)");
    option = Console.ReadLine();
    Console.WriteLine("option: " + option + "len: " + option.Length);

    Console.WriteLine("Whether to list the source code as a comment in the bundle file (true->t, false->f)? (Default->False).");
    if (Console.ReadLine() == "t")
        note = true;
    Console.WriteLine("note: " + note);
    Console.WriteLine("The order of copying the code files, according to the alphabet of the file name or according to the type of code." +
        "\nDefault value will be according to the alphabet of the file name.");
    do
    {
        Console.WriteLine("Enter order of copying the code files,choose-> AlpaBeticOrder, TypeOrder");
        s = Console.ReadLine();

    } while (s != "AlpaBeticOrder" && s != "TypeOrder" && s == null);
    if (s != null)
        sort = s;
    Console.WriteLine("sort :" + sort);
    Console.WriteLine("Do delete empty lines (true->t, false->f)? default -> false");
    if (Console.ReadLine() == "t")
        removeLines = true;
    Console.WriteLine("rempve :" + removeLines);
    Console.WriteLine("Registering the name of the creator of the file.");
    author = Console.ReadLine();
    Console.WriteLine("List of programming languages. \nThe application will include in the bundle only code files of the selected languages." +
        "\nto choose all press All" +
        "choose from among:");
    foreach (var item in languages.Keys)
        Console.Write(item + ", ");
    Console.WriteLine();
    string l = Console.ReadLine();
    l?.Split(' ').ToList().ForEach(l => { if (languages.Keys.Contains(l)) lang.Add(l); });
    create_file(response, option, note, sort, author, removeLines, lang);
    response.Close();
}
void create_file(FileStream response, string option, bool note, string sort, string author, bool removeLines, List<string> langs)
{
    StreamWriter writer = new StreamWriter(response);
    writer.Write("bundle ");
    if (option.Length > 1)
        writer.Write("--o " + option);
    if (sort.Length > 1)
        writer.Write(" --s " + sort);
    if (author.Length > 1)
        writer.Write(" --a " + "\"" + author + "\"");
    if (note)
        writer.Write(" --n");
    if (removeLines)
        writer.Write(" --r");
    writer.Write("--l ");
    langs.ForEach(l => writer.Write(l + " "));
    writer.Close();
}
