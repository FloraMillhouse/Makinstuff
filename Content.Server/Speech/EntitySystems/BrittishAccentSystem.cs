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
