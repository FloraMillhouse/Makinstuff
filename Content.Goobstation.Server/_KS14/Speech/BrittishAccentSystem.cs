using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class BrittishAccentComponentAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly IReadOnlyDictionary<string, string[]> Letters = new Dictionary<string, string[]>()
    {
        { "Hello",  ["Wagwan", "Oi"] },
        { "t",  ["'"] },
        { "Pedo",  ["Nonce"] },
        { "Sec",  ["Feds", "Pigs", "Coppas"] },
        { "Security",  ["Feds", "Pigs", "Coppas"] },
        { "Spesos",  ["quid"] },
        { "Dude", ["bloke"]},
        { "Good", ["Peng"]},
        { "oh no", ["Oh bloody hell!"]},
        { "bad", ["shite"]},
        { "yes", ["go on theeen"]},
        { "Conservitive", ["Tori", "Tory"]},
        { "Cybersun", ["dodgey muckers"]},
        { "ling", ["weird looking bloke"]},
        { "wizard", ["magical bloke"]},
        { "syndicate", ["foreigners"]},
        { "heretic", ["fackin witch"]},
        { "thief", ["slimey bastard"]},
        { "captain", ["nigel"]},
        { "coin", ["penny"]},
        { "no", ["nah mate"]},
        { "clone", ["copycat"]},
        { "devil", ["rich bloke"]},
        { "cultist", ["sum weird religios bloke"]},
    };

    public override void Initialize()
    {
        SubscribeLocalEvent<BrittishAccentComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(string message)
    {
        foreach (var (word, repl) in Letters)
        {
            message = message.Replace(word, _random.Pick(repl));
        }

        return message;
    }

    private void OnAccent(EntityUid uid, BrittishAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
