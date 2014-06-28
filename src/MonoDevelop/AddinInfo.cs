using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly:Addin("GameStack", Namespace="GameStack", Version="1.0.0.0", Category="GameStack")]
[assembly:AddinName("GameStack Project Support")]
[assembly:AddinDescription("Adds project templates for GameStack.")]
[assembly:AddinDependency ("::MonoDevelop.Ide", "5.0.1")]