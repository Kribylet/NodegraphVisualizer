using System;

namespace Nodegraph_Generator
{
    /*
    * Utility methods used in a number of places.
    */
    public static class Util
    {
        // Corresponds to 6 correct decimal points (Double/float precision theory)
        static public readonly double DOUBLE_EPSILON = 0.5 * 1E-6;

        /*
        * Function to compare two doubles, that accepts a small difference (DOUBLE_EPSILON).
        */
        public static bool NearlyEqual(double a, double b)
        {
            if (a.Equals(b))
            { // shortcut, handles infinities (bit equality)
                return true;
            }

            double diff = Math.Abs(a - b);

            // For coordinates we only care about an absolute deviation from another value
            return diff < DOUBLE_EPSILON;
        }

        /*
         * Finds and returns the inverse of a 2D matrix by gaussian elimination.
         */
        public static double[,] Invert(double[,] matrix)
        {
            double[,] lHS = new double[matrix.GetLength(0),matrix.GetLength(1)]; // explicit deep copy

            Array.Copy(matrix, lHS, matrix.Length);

            if (lHS.GetLength(0) != lHS.GetLength(1)) throw new InvalidOperationException("Unable to triangularize non-square matrices.");

            double[,] rHS = new double[lHS.GetLength(0),lHS.GetLength(1)];

            for (int i = 0; i < rHS.GetLength(0); i++)
            {
                rHS[i,i] = 1d;
            }

            return Solve(lHS, rHS);
        }

        /*
         *  Solves equation systems using right-triangularization and back substitution.
         */
        public static double[,] Solve(double[,] lHS, double[,] rHS)
        {
            Triangularize(lHS, rHS);

            BackSubstitute(lHS, rHS);

            return rHS;
        }

        /*
         * Transposes square matrices.
         */
        public static double[,] Transpose(double[,] matrix)
        {
            double[,] transposed = new double[matrix.GetLength(0),matrix.GetLength(1)];
            for (int m = 0; m < matrix.GetLength(0); m++)
            {
                for (int n = 0; n < matrix.GetLength(1); n++)
                {
                    transposed[n,m] = matrix[m,n];
                }
            }
            return transposed;
        }

        /*
         * Performs back substitution on a right-triangularized equation system.
         */
        private static void BackSubstitute(double[,] lHS, double[,] rHS)
        {
            double factor;

            for (int i = lHS.GetLength(0)-1; i >= 0; i--)
            {
                NormalizeRow(lHS, rHS, i);

                for (int j = 0; j < i; j++)
                {
                    factor = lHS[j, i];
                    AddToRows(lHS, rHS, j, i, -factor);
                }
            }
        }

        /*
         * Normalizes a matrix row around a given diagonal element.
         */
        private static void NormalizeRow(double[,] lHS, double[,] rHS, int diagonalIndex)
        {
            double value = lHS[diagonalIndex,diagonalIndex];
            for (int i = 0; i < lHS.GetLength(1); i++)
            {
                lHS[diagonalIndex, i] /= value;
            }

            for (int i = 0; i < rHS.GetLength(1); i++)
            {
                rHS[diagonalIndex, i] /= value;
            }
        }

        /*
         * Transforms equation system into right-triangular form. Assumes matrix dimensionality is square and matches.
         */
        private static void Triangularize(double[,] lHS, double[,] rHS)
        {
            for (int diagIndex = 0; diagIndex < lHS.GetLength(0); diagIndex++)
            {
                RightTriangulateColumn(lHS, rHS, diagIndex);
            }
        }

        /*
         * Makes a matrix column comply with right triangularity. Assumes left-hand columns already
         * comply.
         */
        private static void RightTriangulateColumn(double[,] lHS, double[,] rHS, int diagIndex)
        {
            if (Util.NearlyEqual(lHS[diagIndex,diagIndex], 0))
            {
                int valueRowIndex = diagIndex+1;
                while (valueRowIndex < lHS.GetLength(0) && Util.NearlyEqual(lHS[valueRowIndex,diagIndex],0))
                {
                    valueRowIndex++;
                }

                if (valueRowIndex == lHS.GetLength(0)) throw new InvalidOperationException("Non-singular matrix.");

                AddToRows(lHS, rHS, diagIndex, valueRowIndex, 1d);
            }

            double value = lHS[diagIndex, diagIndex];

            for (int colPos = diagIndex+1; colPos < lHS.GetLength(0); colPos++)
            {
                double factor = lHS[colPos, diagIndex]/value;

                AddToRows(lHS, rHS, colPos, diagIndex, -factor);
            }
        }

        /*
         * Helper function that performs left- and right-hand side equation system operations at the same time.
         */
        private static void AddToRows(double[,] lHS, double[,] rHS, int rowIndex, int valueRowIndex, double factor = 1d)
        {
            double[] lHSRow = GetRow(lHS, valueRowIndex, factor);
            double[] rHSRow = GetRow(rHS, valueRowIndex, factor);

            for (int i = 0; i < lHSRow.Length; i++)
            {
                lHS[rowIndex, i] += lHSRow[i];
            }

            for (int i = 0; i < rHSRow.Length; i++)
            {
                rHS[rowIndex, i] += rHSRow[i];
            }
        }

        /*
         * Helper function that retrieves a matrix row.
         */
        private static double[] GetRow(double[,] matrix, int valueRowIndex, double factor)
        {
            double[] row = new double[matrix.GetLength(1)];
            for (int i = 0; i < row.GetLength(0); i++)
            {
                row[i] = matrix[valueRowIndex, i] * factor;
            }
            return row;
        }

        /*
         * Calculates the dot product of a matrix and vector. Returns a vector.
         */
        public static double[] Dot(double[,] matrix, double[] vector)
        {
            if (matrix.GetLength(1) != vector.GetLength(0)) throw new InvalidOperationException("Dimensions must match.");

            double[] dotProduct = new double[vector.GetLength(0)];

            for (int m = 0; m < vector.GetLength(0); m++)
            {
                for (int n = 0; n < matrix.GetLength(1); n++)
                {
                    dotProduct[m] += matrix[m,n]*vector[n];
                }
            }

            return dotProduct;
        }

        /*
         * Calculates the dot product of a matrix and vector. Returns Vect3.
         */
        public static Vect3 Dot(double[,] matrix, Vect3 vector)
        {
            if (matrix.GetLength(1) != 3) throw new InvalidOperationException("Dimensions must match.");

            Vect3 dotVector = new Vect3();

            for (int m = 0; m < 3; m++)
            {
                for (int n = 0; n < matrix.GetLength(1); n++)
                {
                    dotVector[m] += matrix[m,n]*vector[n];
                }
            }

            return dotVector;
        }
    }
}