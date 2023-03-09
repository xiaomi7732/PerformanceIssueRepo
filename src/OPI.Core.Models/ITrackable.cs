using System;

namespace OPI.Core.Models;

public interface ITrackable
{
    DateTime? CreatedAt { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? CreatedBy { get; set; }
    string? LastModifiedBy { get; set; }
}