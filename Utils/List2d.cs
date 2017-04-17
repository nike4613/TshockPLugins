using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class List2d<T> : IEnumerable<List<T>>
    {

        private int capx;
        private int capy;
        private List<List<T>> list;

        public int SublistInitCapacity
        {
            get
            {
                return capy;
            }
            set
            {
                capy = value;
            }
        }

        public List2d() : this(16, 16)
        {
        }

        public List2d(int capx, int capy)
        {
            this.capx = capx;
            this.capy = capy;
            list = new List<List<T>>(capx);
        }

        public void Add(int pos, T item)
        {
            EnsurePosition(pos);
            list[pos].Add(item);
        }

        public void EnsurePosition(int pos)
        {
            while (list.Count <= pos)
            {
                AddYDirection();
            }
        }

        public void AddYDirection()
        {
            list.Add(new List<T>(capy));
        }

        public IEnumerator<List<T>> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public List<T> this[int i]
        {
            get
            {
                return list[i];
            }
            set
            {
                list[i] = value;
            }
        }

        public T this[int i, int j]
        {
            get
            {
                return this[i][j];
            }
            set
            {
                this[i][j] = value;
            }
        }

    }
}
