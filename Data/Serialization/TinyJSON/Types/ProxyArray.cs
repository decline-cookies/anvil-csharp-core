using System.Collections;
using System.Collections.Generic;

namespace TinyJSON
{
	public class ProxyArray : Variant, IEnumerable<Variant>
	{
		readonly List<Variant> list;


		public ProxyArray()
		{
			list = new List<Variant>();
		}


		IEnumerator<Variant> IEnumerable<Variant>.GetEnumerator()
		{
			return list.GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}


		public void Add( Variant item )
		{
			list.Add( item );
		}


		public override Variant this[ int index ]
		{
			get => list[index];
            set => list[index] = value;
        }


		public int Count => list.Count;


        internal bool CanBeMultiRankArray( int[] rankLengths )
		{
			return CanBeMultiRankArray( 0, rankLengths );
		}


        private bool CanBeMultiRankArray( int rank, int[] rankLengths )
		{
			int count = list.Count;
			rankLengths[rank] = count;

			if (rank == rankLengths.Length - 1)
			{
				return true;
			}

            if (!(list[0] is ProxyArray firstItem))
			{
				return false;
			}

			int firstItemCount = firstItem.Count;

			for (int i = 1; i < count; i++)
			{
                if (!(list[i] is ProxyArray item))
				{
					return false;
				}

				if (item.Count != firstItemCount)
				{
					return false;
				}

				if (!item.CanBeMultiRankArray( rank + 1, rankLengths ))
				{
					return false;
				}
			}

			return true;
		}
	}
}
