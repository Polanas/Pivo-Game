namespace Game;

struct Tag
{
    public string value;

    public Tag(string tag) => value = tag;

    public static bool operator ==(Tag tag, string str) => tag.value == str;

    public static bool operator !=(Tag tag, string str) => tag.value != str;

    public override bool Equals(object obj) => Equals(obj);

    public override int GetHashCode() => GetHashCode();
}
