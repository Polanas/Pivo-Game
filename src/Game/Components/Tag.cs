namespace Game;

struct Tag
{
    public string value;

    public Tag(string tag) => this.value = tag;

    public static bool operator ==(Tag tag, string str) => tag.value == str;

    public static bool operator !=(Tag tag, string str) => tag.value != str;
}
