using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class BigotAccentComponentAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly IReadOnlyDictionary<string, string[]> Letters = new Dictionary<string, string[]>()
    {
        { "Hello",  ["AY DICKHEAD"] },
        { "Hi",  ["OI YOU PIXIE"] },
        { "Lizard",  ["LIGGA"] },
        { "Moth",  ["LAMP-OBSESED-PIXIE"] },
        { "gay",  ["NOB"] },
        { "I",  ["I, A TRUE REAL PERSON,"] },
        { "Cargo",  ["MONEY HOARDING", "BANKERS"] },
        { "QM",  ["THE GLOBAL ELITE"] },
    };

    public override void Initialize()
    {
        SubscribeLocalEvent<BigotAccentComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(string message)
    {
        foreach (var (word, repl) in Letters)
        {
            message = message.Replace(word, _random.Pick(repl));
        }

        return message;
    }

    private void OnAccent(EntityUid uid, BigotAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
