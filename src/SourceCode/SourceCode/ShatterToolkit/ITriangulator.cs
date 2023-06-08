namespace ShatterToolkit;

public interface ITriangulator
{
	void Fill(out int[] newEdges, out int[] newTriangles, out int[] newTriangleEdges);
}
