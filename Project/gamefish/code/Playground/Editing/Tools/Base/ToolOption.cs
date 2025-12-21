using System;

namespace Playground;

/// <summary>
/// Tells the UI to show a property as a tool option.
/// </summary>
[AttributeUsage( AttributeTargets.Property )]
public partial class ToolOptionAttribute : Attribute;
