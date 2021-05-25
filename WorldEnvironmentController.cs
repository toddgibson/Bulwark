using Godot;
using System;

public class WorldEnvironmentController : WorldEnvironment
{
	[Export] private bool EnableSSAO;
	[Export] private bool EnableDOF;
	[Export] private bool EnableFog;

	public override void _Ready()
	{
		UpdateSettings();
	}

	public void UpdateSettings()
	{
		Environment.FogEnabled = EnableFog;
		Environment.SsaoEnabled = EnableSSAO;
		Environment.DofBlurFarEnabled = EnableDOF;
		Environment.DofBlurNearEnabled = EnableDOF;
	}
}
