using Microsoft.AspNetCore.Components;
using OPI.Core.Models;

namespace OPI.WebUI.Parts;

public partial class TrackableViewer
{
    [Parameter]
    public ITrackable? Trackable { get; set; }
}