using System.Collections.Generic;
using ProtoBuf;

namespace ProspectorsXRay.Network;

[ProtoContract]
public class SurveyResultPacket
{
    [ProtoMember(1)]
    public int CenterX { get; set; }

    [ProtoMember(2)]
    public int CenterY { get; set; }

    [ProtoMember(3)]
    public int CenterZ { get; set; }

    [ProtoMember(4)]
    public int Radius { get; set; }

    [ProtoMember(5)]
    public string ModeName { get; set; } = string.Empty;

    [ProtoMember(6)]
    public int TotalFoundCount { get; set; }

    [ProtoMember(7)]
    public Dictionary<string, int> TotalCounts { get; set; } = new();

    [ProtoMember(8)]
    public Dictionary<string, int> ShownCounts { get; set; } = new();

    [ProtoMember(9)]
    public List<int> RevealedX { get; set; } = new();

    [ProtoMember(10)]
    public List<int> RevealedY { get; set; } = new();

    [ProtoMember(11)]
    public List<int> RevealedZ { get; set; } = new();

    [ProtoMember(12)]
    public List<int> RevealedColors { get; set; } = new();

    [ProtoMember(13)]
    public List<string> RevealedMinerals { get; set; } = new();

    [ProtoMember(14)]
    public List<int> HiddenX { get; set; } = new();

    [ProtoMember(15)]
    public List<int> HiddenY { get; set; } = new();

    [ProtoMember(16)]
    public List<int> HiddenZ { get; set; } = new();

    [ProtoMember(17)]
    public List<int> HiddenColors { get; set; } = new();

    [ProtoMember(18)]
    public List<string> HiddenMinerals { get; set; } = new();
}
