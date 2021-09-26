using MessagePack;

namespace Proto.Serialization.MessagePack
{
    //user messages

    [MessagePackObject]
    public class PoisonPill
    {
    }

    [MessagePackObject]
    public class DeadLetterResponse
    {
        [Key(0)] public PID Target { get; set; }
    }


    //system messages

    [MessagePackObject]
    public class Watch
    {
        [Key(0)] public PID Watcher { get; set; }
    }

    [MessagePackObject]
    public class Unwatch
    {
        [Key(0)] public PID Watcher { get; set; }
    }

    [MessagePackObject]
    public class Terminated
    {
        [Key(0)] public PID who { get; set; }
        [Key(1)] public TerminatedReason Why { get; set; }
    }

    public enum TerminatedReason
    {
        Stopped = 0,
        AddressTerminated = 1,
        NotFound = 2
    }


    [MessagePackObject]
    public class Stop
    {
    }

    [MessagePackObject]
    public class Touch
    {
    }

    [MessagePackObject]
    public class Touched
    {
        [Key(0)] public PID who { get; set; }
    }
}