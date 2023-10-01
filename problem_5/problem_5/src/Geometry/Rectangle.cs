namespace problem_4.Geometry;

public readonly record struct Rectangle(Point2D LeftBottom, Point2D RightTop)
{
    public Point2D LeftTop { get; } = new(LeftBottom.R, RightTop.Z);
    public Point2D RightBottom { get; } = new(RightTop.R, LeftBottom.Z);
}