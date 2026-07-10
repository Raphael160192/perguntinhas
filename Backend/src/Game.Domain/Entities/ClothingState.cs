using Game.Domain.Enums;

namespace Game.Domain.Entities;

public class ClothingState
{
    public bool Socks { get; set; } = true;
    public bool Shirt { get; set; } = true;
    public bool Pants { get; set; } = true;
    public bool Underwear { get; set; } = true;

    public bool HasItem(ClothingItem item) => item switch
    {
        ClothingItem.Socks => Socks,
        ClothingItem.Shirt => Shirt,
        ClothingItem.Pants => Pants,
        ClothingItem.Underwear => Underwear,
        _ => false
    };

    public void LoseItem(ClothingItem item)
    {
        switch (item)
        {
            case ClothingItem.Socks: Socks = false; break;
            case ClothingItem.Shirt: Shirt = false; break;
            case ClothingItem.Pants: Pants = false; break;
            case ClothingItem.Underwear: Underwear = false; break;
        }
    }

    public int RemainingCount() =>
        (Socks ? 1 : 0) + (Shirt ? 1 : 0) + (Pants ? 1 : 0) + (Underwear ? 1 : 0);
}
