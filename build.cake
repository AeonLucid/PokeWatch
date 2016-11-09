var projectName = "PokeWatch";
var version = EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "1.0.0";
var sourceDirectory = "./src/PokeWatch";
var outputDirectory = "./output";
var targets = new string[][]
{
    new string[] { "net45", "win7-x86", "x86" }, 
    new string[] { "net45", "win7-x64", "x64" }
};

Task("Clean").Does(() => 
{
	if (DirectoryExists(outputDirectory)) {
		DeleteDirectory(outputDirectory, recursive:true);
	}

	CreateDirectory(outputDirectory);
});

Task("Version").Does(() =>
{
	if (AppVeyor.IsRunningOnAppVeyor) {
		if (AppVeyor.Environment.Repository.Branch.Contains("develop")) {
			version = version + "-dev";
		}

		Information("Running on AppVeyor, patching the version to '" + version + "'...");
		
		var projectJson = sourceDirectory + "/project.json";
		var updatedProjectJson = System.IO.File.ReadAllText(projectJson).Replace("1.0.0-*", version);

		// System.IO.File.WriteAllText(projectJson, updatedProjectJson);
	} else {
		Information("Not running on AppVeyor, skipping version patch...");
	}
});

Task("Build")
	.Does(() => 
{
	DotNetCoreRestore(sourceDirectory);

	foreach (var target in targets)
	{
		var targetDirectory = System.IO.Path.Combine(outputDirectory, target[1]);
		var settings = new DotNetCorePublishSettings
		{
			Framework = target[0],
			Runtime = target[1],
			Configuration = "Release",
			OutputDirectory = targetDirectory
		};

		DotNetCorePublish(sourceDirectory, settings);
	}      
});

Task("Archive")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.IsDependentOn("Build")
	.Does(() => 
{
	foreach (var target in targets)
	{
		var targetDirectory = System.IO.Path.Combine(outputDirectory, target[1]);
		var targetZip = System.IO.Path.Combine(outputDirectory, projectName + "-" + target[2] + "-v" + version + ".zip");

		Zip(targetDirectory, targetZip);
	}
});

RunTarget("Archive");