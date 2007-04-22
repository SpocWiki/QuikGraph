using System;
using MbUnit.Framework;

namespace QuickGraph.Algorithms
{
    public class DoubleDenseMatrixFactory
    {
        [Factory]
        public DoubleDenseMatrix Scalar()
        {
            return new DoubleDenseMatrix(1, 1, 1);
        }

        [Factory]
        public DoubleDenseMatrix Identity2()
        {
            return DoubleDenseMatrix.Identity(2);        
        }

        [Factory]
        public DoubleDenseMatrix Zero2()
        {
            return new DoubleDenseMatrix(2, 2);
        }

        [Factory]
        public DoubleDenseMatrix Chain()
        {
            DoubleDenseMatrix m = new DoubleDenseMatrix(3, 3);
            m[0, 1] = 1;
            m[1, 2] = 1;
            return m;
        }

        [Factory]
        public DoubleDenseMatrix Evenly23()
        {
            DoubleDenseMatrix m = new DoubleDenseMatrix(2, 3);
            int k = 0;
            for (int i = 0; i < m.RowCount; ++i)
                for (int j = 0; j < m.ColumnCount; ++j)
                    m[i, j] = k++;
            return m;
        }
    }
}
