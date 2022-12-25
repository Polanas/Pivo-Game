using System;
using System.Xml.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL4;

namespace Game;

struct Rect
{
    public Vector2i pos;
    public Vector2i size;
    public Texture texture;
    public int frame;

    public Rect(Vector2i pos, Vector2i size, int frame, Texture texture)
    {
        this.size = size;
        this.pos = pos;
        this.frame = frame;
        this.texture = texture;
    }

    public Rect(Vector2i pos, Vector2i size)
    {
        this.size = size;
        this.pos = pos;
        frame = -1;
        texture = null;
    }
}

class Atlas
{
    struct EmptySpaces
    {
        public Rect biggerSplit;
        public Rect lesserSplit;
        public readonly int count;

        public EmptySpaces(Rect biggerSplit, Rect lesserSplit, int count)
        {
            this.biggerSplit = biggerSplit;
            this.lesserSplit = lesserSplit;
            this.count = count;
        }

        public EmptySpaces(Rect split)
        {
            biggerSplit = lesserSplit = split;
            count = 1;
        }

        public static EmptySpaces GetEmpty() => new EmptySpaces(default, default, -1);
    }


    public List<Rect> Rects { get; private set; }

    public Vector2i TextureSize { get; private set; }

    private static Comparison<Rect> _PathologicaMultiplierComparsion = new((r1, r2) =>
    {
        float mult1 = r1.size.X * r1.size.Y;
        float mult2 = r2.size.X * r2.size.Y;

        if (mult2 > mult1)
            return 1;
        else if (mult1 == mult2)
            return 0;
        else return -1;
    });

    public static Atlas FromDirectories(Vector2i textureSize, string metaDataPath, string[] texturePaths)
    {
        Dictionary<Texture, Vector2i> texturesData = new();

        for (int i = 0; i < texturePaths.Length; i++)
        {
            var path = texturePaths[i];

            foreach (var files in Directory.GetFiles(path))
            {
                var fileInfo = new FileInfo(files);
                var texture = Content.LoadTexture(fileInfo.FullName);

                texturesData.Add(texture, new Vector2i(-1));
            }
        }

        foreach (var file in Directory.GetFiles(metaDataPath))
        {
            var fileInfo = new FileInfo(file);
            JObject jobject = JObject.Parse(File.ReadAllText(fileInfo.FullName));

            var frames = jobject["frames"];
            var frameSizeJObject = frames.First.First["sourceSize"];

            Vector2i frameSize = new Vector2i
            {
                X = frameSizeJObject["w"].Value<int>(),
                Y = frameSizeJObject["h"].Value<int>()
            };

            string fileName = Path.GetFileNameWithoutExtension(file);
            var texture = Content.GetTexture(fileName);

            if (texture.name != fileName)
                continue;

            texturesData[texture] = frameSize;
        }

        var rects = new RefList<Rect>();

        foreach (var pair in texturesData)
        {
            var frameSize = pair.Value;
            var texture = pair.Key;

            if (frameSize.X != -1 && frameSize.Y != -1)
            {
                for (int i = 0; i < (texture.Width / frameSize.X * (texture.Height / frameSize.Y)); i++)
                    rects.Add(new Rect(Vector2i.Zero, frameSize, i, texture));

                continue;
            }

            rects.Add(new Rect(Vector2i.Zero, texture.Size, 0, texture));
        }

        var atlas = new Atlas(textureSize);
        atlas.PackGullotine(rects);

        return atlas;
    }

    public Atlas(Vector2i textureSize)
    {
        TextureSize = textureSize;
        Rects = new();
    }

    private void PackGullotine(RefList<Rect> rects)
    {
        rects.Sort(_PathologicaMultiplierComparsion);

        var emptySpaces = new RefList<Rect>();
        emptySpaces.Add(new Rect(default, TextureSize));

        for (int i = 0; i < rects.Count; i++)
        {
            ref var rect = ref rects[i];

            for (int j = emptySpaces.Count - 1; j >= 0; j--)
            {
                var emptySpace = emptySpaces[j];
                if (rect.size.X <= emptySpace.size.X && rect.size.Y <= emptySpace.size.Y)
                {
                    var newEmptySpaces = InsertAndSplit(rect, emptySpace);

                    rect.pos = emptySpace.pos;
                    Rects.Add(new Rect(rect.pos, rect.size, rect.frame, rect.texture));

                    emptySpaces.RemoveAt(j);

                    if (newEmptySpaces.count < 0)
                        break;

                    if (newEmptySpaces.count == 1)
                    {
                        emptySpaces.Add(newEmptySpaces.biggerSplit);
                        break;
                    }

                    emptySpaces.Add(newEmptySpaces.biggerSplit);
                    emptySpaces.Add(newEmptySpaces.lesserSplit);

                    break;
                }

            }
        }
    }

    private EmptySpaces InsertAndSplit(Rect imageRect, Rect spaceRect)
    {
        var freeRectSize = spaceRect.size - imageRect.size;

        if (freeRectSize.X < 0 && freeRectSize.Y < 0)
            throw new Exception("Packing failed: there was not enough space");

        if (freeRectSize.X == 0 && freeRectSize.Y == 0)
            return EmptySpaces.GetEmpty();

        if (freeRectSize.X > 0 && freeRectSize.Y == 0)
        {
            var rect = spaceRect;
            rect.pos.X += imageRect.size.X;
            rect.size.X -= imageRect.size.X;

            return new EmptySpaces(rect);
        }
        if (freeRectSize.X == 0 && freeRectSize.Y > 0)
        {
            var rect = spaceRect;
            rect.pos.Y += imageRect.size.Y;
            rect.size.Y -= imageRect.size.Y;

            return new EmptySpaces(rect);
        }

        Rect biggerSplit, lesserSplit;

        if (freeRectSize.X > freeRectSize.Y)
        {
            biggerSplit = new Rect
            {
                pos = new Vector2i(spaceRect.pos.X + imageRect.size.X, spaceRect.pos.Y),
                size = new Vector2i(freeRectSize.X, spaceRect.size.Y)
            };
            lesserSplit = new Rect
            {
                pos = new Vector2i(spaceRect.pos.X, spaceRect.pos.Y + imageRect.size.Y),
                size = new Vector2i(imageRect.size.X, freeRectSize.Y)
            };

            return new EmptySpaces(biggerSplit, lesserSplit, 2);
        }

        biggerSplit = new Rect
        {
            pos = new Vector2i(spaceRect.pos.X, spaceRect.pos.Y + imageRect.size.Y),
            size = new Vector2i(spaceRect.size.X, freeRectSize.Y)
        };
        lesserSplit = new Rect
        {
            pos = new Vector2i(spaceRect.pos.X + imageRect.size.X, spaceRect.pos.Y),
            size = new Vector2i(freeRectSize.X, imageRect.size.Y)
        };

        return new EmptySpaces(biggerSplit, lesserSplit, 2);
    }

    //private void PackNaive(RefList<Rect> rects)
    //{
    //    rects.Sort(_PathologicaMultiplierComparsion);

    //    int xPos = 0;
    //    int yPos = 0;
    //    int largestHeghtInRow = 0;

    //    for (int i = 0; i < rects.Count; i++)
    //    {
    //        ref var rect = ref rects[i];

    //        if (xPos + rect.size.X > TextureSize.X)
    //        {
    //            yPos += largestHeghtInRow;
    //            xPos = largestHeghtInRow = 0;
    //        }

    //        if (yPos + rect.size.Y > TextureSize.Y)
    //            throw new Exception("Construction of an atlas was failed.");

    //        rect.pos = new Vector2i(xPos, yPos);

    //        xPos += rect.size.X;

    //        if (rect.size.Y > largestHeghtInRow)
    //            largestHeghtInRow = rect.size.Y;

    //        FinalRects.Add(new FrameRect { Position = rect.pos, Size = rect.size });
    //    }
    //}
}
