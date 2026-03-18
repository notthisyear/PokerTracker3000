using System.ComponentModel;

namespace PokerTracker3000.GameSession.Sound
{
    public enum SoundEffectType
    {
        None,
        [Description("Built-in")]
        BuiltIn,
        [Description("Synthesized speech")]
        Speech,
        [Description("From file")]
        File,
    }

    public enum BuiltInSoundEffectType
    {
        None,
        [Description("Blues riff")]
        Riff,
    }

    public enum MultipleOptionMode
    {
        None,
        [Description("Sequential")]
        Sequential,
        [Description("Shuffle")]
        Shuffle,
        [Description("Random")]
        Random
    }
}
