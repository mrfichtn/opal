namespace Opal
{
    ///<summary>Segment location in a file</summary>
    public class Segment
	{
		public Position Start;
		public Position End;

		public Segment() { }

		public Segment(Segment cpy)
		{
			if (cpy != null)
			{
				Start = cpy.Start;
				End = cpy.End;
			}
		}

		public Segment(Position start)
		{
			Start = start;
			End = start;
		}

		public Segment(Position start, Position end)
		{
			Start = start;
			End = end;
		}

		public bool IsEmpty => (End == Start);
		public int Beg => Start.Ch;
		public int Length => End.Ch - Start.Ch + 1;

		public void CopyFrom(Segment segment)
		{
			if (segment != null)
			{
				Start = segment.Start;
				End = segment.End;
			}
		}
	}
}
