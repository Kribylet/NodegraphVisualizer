
using System;
using System.Collections.Generic;
using System.IO;

namespace Nodegraph_Generator
{
    public enum PointType:int
    {
        IGNORED = 0,
        BLACK,
        WHITE,
        X
    }
    public enum PlaneEnum
    {
        XY,
        XZ,
        YZ
    }
    public enum Reflection
    {
        R1,
        R2,
        R3
    }

    /**
     * Contains static templates used when checking if a voxel is to be removed.
     * 
     * The templates used are derived from the paper:
     * Palágyi K., Kuba A. (1999) Directional 3D Thinning Using 8 Subiterations. 
     * In: Bertrand G., Couprie M., Perroton L. (eds) Discrete Geometry for Computer Imagery. DGCI 1999. 
     * Lecture Notes in Computer Science, vol 1568. Springer, Berlin, Heidelberg
     */

    public class GridTemplate
    {
        public static readonly PointType[][][] b1USW = new PointType[][][] /*x y z*/
        {
            new PointType[][]
            {
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },
            new PointType[][]
            {

                new PointType[]{PointType.WHITE,PointType.X,PointType.X},
                new PointType[]{PointType.WHITE,PointType.BLACK,PointType.X},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },
            new PointType[][]
            {

                new PointType[]{PointType.WHITE,PointType.X,PointType.BLACK},
                new PointType[]{PointType.WHITE,PointType.X,PointType.X},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            }
        };
        public static readonly PointType[][][] b2USW = new PointType[][][] /*x y z*/
        {
            new PointType[][]
            {

                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },

            new PointType[][]
            {

                new PointType[]{PointType.X,PointType.X,PointType.X},
                new PointType[]{PointType.X,PointType.BLACK,PointType.X},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },

            new PointType[][]
            {
                new PointType[]{PointType.X,PointType.BLACK,PointType.X},
                new PointType[]{PointType.X,PointType.X,PointType.X},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            }
        };
        public static readonly PointType[][][] b3USW = new PointType[][][] /*x y z*/
        {
            new PointType[][]
            {

                new PointType[]{PointType.X,PointType.X,PointType.X},
                new PointType[]{PointType.X,PointType.X,PointType.X},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },

            new PointType[][]
            {

                new PointType[]{PointType.X,PointType.BLACK,PointType.X},
                new PointType[]{PointType.X,PointType.BLACK,PointType.X},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },

            new PointType[][]
            {

                new PointType[]{PointType.X,PointType.X,PointType.X},
                new PointType[]{PointType.X,PointType.X,PointType.X},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            }
        };
        public static readonly PointType[][][] b4USW = new PointType[][][] /*x y z*/
        {

            new PointType[][]
            {
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.IGNORED},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.IGNORED},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },

            new PointType[][]
            {
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.BLACK},
                new PointType[]{PointType.WHITE,PointType.BLACK,PointType.IGNORED},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },

            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.BLACK,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            }
        };
        public static readonly PointType[][][] b5USW = new PointType[][][] /*x y z*/
        {
            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.IGNORED}
            },
            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.BLACK,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.BLACK,PointType.BLACK},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.IGNORED}
            },
            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.IGNORED}
            }
        };
        public static readonly PointType[][][] b6USW = new PointType[][][] /*x y z*/
        {
            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.IGNORED}
            },
            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.BLACK,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.BLACK,PointType.BLACK},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.IGNORED}
            },
            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.BLACK,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED}
            }
        };
        public static readonly PointType[][][] b7USW = new PointType[][][] /*x y z*/
        {
            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },
            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.BLACK,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.BLACK,PointType.IGNORED},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },
            new PointType[][]
            {
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.BLACK},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.BLACK}
            }
        };

        private static List<GridTemplate> basicUSWGridTemplates = new List<GridTemplate>()
        {
            new GridTemplate(b1USW),
            new GridTemplate(b2USW),
            new GridTemplate(b3USW),
            new GridTemplate(b4USW),
            new GridTemplate(b5USW),
            new GridTemplate(b6USW),
            new GridTemplate(b7USW),
        };

        public static List<GridTemplate> USWGridTemplates = GenerateReflections(basicUSWGridTemplates);

        public static List<GridTemplate> USEGridTemplates = MirrorAllGridTemplates(USWGridTemplates, PlaneEnum.YZ);
        public static List<GridTemplate> UNWGridTemplates = MirrorAllGridTemplates(USWGridTemplates, PlaneEnum.XY);
        public static List<GridTemplate> UNEGridTemplates = MirrorAllGridTemplates(UNWGridTemplates, PlaneEnum.YZ);
        public static List<GridTemplate> DSWGridTemplates = MirrorAllGridTemplates(USWGridTemplates, PlaneEnum.XZ);
        public static List<GridTemplate> DSEGridTemplates = MirrorAllGridTemplates(DSWGridTemplates, PlaneEnum.YZ);
        public static List<GridTemplate> DNWGridTemplates = MirrorAllGridTemplates(DSWGridTemplates, PlaneEnum.XY);
        public static List<GridTemplate> DNEGridTemplates = MirrorAllGridTemplates(DNWGridTemplates, PlaneEnum.YZ);

        public static GridTemplate NeighborLessTemplate = new GridTemplate(new PointType[][][] /*x y z*/
        {
            new PointType[][]
            {
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },
            new PointType[][]
            {

                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.BLACK,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            },
            new PointType[][]
            {

                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE},
                new PointType[]{PointType.WHITE,PointType.WHITE,PointType.WHITE}
            }
        });
        private static GridTemplate FillableTemplate = new GridTemplate(new PointType[][][] /*x y z*/
        {
            new PointType[][]
            {
                new PointType[]{PointType.BLACK,PointType.BLACK,PointType.BLACK},
                new PointType[]{PointType.BLACK,PointType.IGNORED,PointType.BLACK},
                new PointType[]{PointType.BLACK,PointType.BLACK,PointType.BLACK}
            },
            new PointType[][]
            {

                new PointType[]{PointType.BLACK,PointType.BLACK,PointType.BLACK},
                new PointType[]{PointType.IGNORED,PointType.WHITE,PointType.IGNORED},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.BLACK}
            },
            new PointType[][]
            {

                new PointType[]{PointType.BLACK,PointType.BLACK,PointType.BLACK},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.BLACK},
                new PointType[]{PointType.IGNORED,PointType.IGNORED,PointType.BLACK}
            }
        });

        private static GridTemplate FillableTemplateX = MirrorInPlane(FillableTemplate, PlaneEnum.YZ);
        private static GridTemplate FillableTemplateY = MirrorInPlane(FillableTemplate, PlaneEnum.XZ);
        private static GridTemplate FillableTemplateZ = MirrorInPlane(FillableTemplate, PlaneEnum.XY);
        private static GridTemplate FillableTemplateXY = MirrorInPlane(FillableTemplateX, PlaneEnum.XZ);
        private static GridTemplate FillableTemplateXZ = MirrorInPlane(FillableTemplateX, PlaneEnum.XY);
        private static GridTemplate FillableTemplateYZ = MirrorInPlane(FillableTemplateY, PlaneEnum.XY);
        private static GridTemplate FillableTemplateXYZ = MirrorInPlane(FillableTemplateXY, PlaneEnum.XY);

        public static List<GridTemplate> FillableTemplates = new List<GridTemplate>()
        {
            FillableTemplate,
            FillableTemplateX,
            FillableTemplateY,
            FillableTemplateZ,
            FillableTemplateXY,
            FillableTemplateXZ,
            FillableTemplateYZ,
            FillableTemplateXYZ
        };

        public PointType[][][] pointMask;

        public GridTemplate()
        {
            pointMask = new PointType[3][][];
            for (int i = 0; i < pointMask.Length; i++)
            {
                pointMask[i] = new PointType[3][];

                for (int j = 0; j < pointMask[i].Length; j++)
                {
                    pointMask[i][j] = new PointType[3];
                }
            }
        }

        public GridTemplate(PointType[][][] preset)
        {
            pointMask = new PointType[3][][];
            CopyPointMask(preset, ref pointMask);
        }

        public GridTemplate(GridTemplate gridTemplate)
        {
            CopyPointMask(gridTemplate.pointMask, ref pointMask);
        }

        private void CopyPointMask(PointType[][][] preset, ref PointType[][][] pointMask)
        {
            for (int i = 0; i < pointMask.Length; i++)
            {
                pointMask[i] = new PointType[3][];

                for (int j = 0; j < pointMask[i].Length; j++)
                {
                    pointMask[i][j] = new PointType[3];
                    Array.Copy(preset[i][j], pointMask[i][j], 3);
                }
            }
        }

        public static GridTemplate MirrorInPlane(GridTemplate other, PlaneEnum plane)
        {
            GridTemplate newGridTemplate = new GridTemplate(other.pointMask);

            switch (plane)
            {
                case PlaneEnum.XY:
                    for (int x = 0; x < newGridTemplate.pointMask.Length; x++)
                    {
                        for (int y = 0; y < newGridTemplate.pointMask[0].Length; y++)
                        {
                            PointType tempPointTypePoint = newGridTemplate.pointMask[x][y][0];
                            newGridTemplate.pointMask[x][y][0] = newGridTemplate.pointMask[x][y][2];
                            newGridTemplate.pointMask[x][y][2] = tempPointTypePoint;
                        }
                    }
                    break;
                case PlaneEnum.XZ:
                    for (int x = 0; x < newGridTemplate.pointMask.Length; x++)
                    {
                        PointType[] tempPointTypeRow = newGridTemplate.pointMask[x][0];
                        newGridTemplate.pointMask[x][0] = newGridTemplate.pointMask[x][2];
                        newGridTemplate.pointMask[x][2] = tempPointTypeRow;
                    }
                    break;
                case PlaneEnum.YZ:
                    PointType[][] tempPointTypePlane = newGridTemplate.pointMask[0];
                    newGridTemplate.pointMask[0] = newGridTemplate.pointMask[2];
                    newGridTemplate.pointMask[2] = tempPointTypePlane;
                    break;
            }

            return newGridTemplate;
        }

        public static GridTemplate Reflect(GridTemplate other, Reflection reflection)
        {
            return Reflect(other.pointMask, reflection);
        }

        public static GridTemplate Reflect(PointType[][][] pMask, Reflection reflection)
        {
            GridTemplate newGridTemplate = new GridTemplate(pMask);

            PointType tempPointTypePoint;
            PointType[] tempPointTypeRow;

            switch (reflection)
            {
                case Reflection.R1:
                    for (int y = 0; y < newGridTemplate.pointMask[0].Length; y++)
                    {
                        tempPointTypePoint = newGridTemplate.pointMask[0][y][1];
                        newGridTemplate.pointMask[0][y][1] = newGridTemplate.pointMask[1][y][0];
                        newGridTemplate.pointMask[1][y][0] = tempPointTypePoint;

                        tempPointTypePoint = newGridTemplate.pointMask[0][y][2];
                        newGridTemplate.pointMask[0][y][2] = newGridTemplate.pointMask[2][y][0];
                        newGridTemplate.pointMask[2][y][0] = tempPointTypePoint;

                        tempPointTypePoint = newGridTemplate.pointMask[1][y][2];
                        newGridTemplate.pointMask[1][y][2] = newGridTemplate.pointMask[2][y][1];
                        newGridTemplate.pointMask[2][y][1] = tempPointTypePoint;
                    }
                    break;
                case Reflection.R2:
                    for (int x = 0; x < newGridTemplate.pointMask[0].Length; x++)
                    {
                        tempPointTypePoint = newGridTemplate.pointMask[x][0][0];
                        newGridTemplate.pointMask[x][0][0] = newGridTemplate.pointMask[x][2][2];
                        newGridTemplate.pointMask[x][2][2] = tempPointTypePoint;

                        tempPointTypePoint = newGridTemplate.pointMask[x][0][1];
                        newGridTemplate.pointMask[x][0][1] = newGridTemplate.pointMask[x][1][2];
                        newGridTemplate.pointMask[x][1][2] = tempPointTypePoint;

                        tempPointTypePoint = newGridTemplate.pointMask[x][1][0];
                        newGridTemplate.pointMask[x][1][0] = newGridTemplate.pointMask[x][2][1];
                        newGridTemplate.pointMask[x][2][1] = tempPointTypePoint;
                    }
                    break;
                case Reflection.R3:
                    tempPointTypeRow = newGridTemplate.pointMask[0][0];
                    newGridTemplate.pointMask[0][0] = newGridTemplate.pointMask[2][2];
                    newGridTemplate.pointMask[2][2] = tempPointTypeRow;

                    tempPointTypeRow = newGridTemplate.pointMask[0][1];
                    newGridTemplate.pointMask[0][1] = newGridTemplate.pointMask[1][2];
                    newGridTemplate.pointMask[1][2] = tempPointTypeRow;

                    tempPointTypeRow = newGridTemplate.pointMask[1][0];
                    newGridTemplate.pointMask[1][0] = newGridTemplate.pointMask[2][1];
                    newGridTemplate.pointMask[2][1] = tempPointTypeRow;
                    break;
            }
            return newGridTemplate;
        }

        public static List<GridTemplate> MirrorAllGridTemplates(List<GridTemplate> gridTemplates, PlaneEnum plane)
        {
            List<GridTemplate> newGridTemplates = new List<GridTemplate>();
            foreach (GridTemplate gridTemplate in gridTemplates)
            {
                newGridTemplates.Add(MirrorInPlane(gridTemplate, plane));
            }

            return newGridTemplates;
        }

        public static List<GridTemplate> GenerateReflections(List<GridTemplate> gridTemplates)
        {
            List<GridTemplate> extendedGridTemplates = new List<GridTemplate>(gridTemplates);

            foreach (var gridTemplate in gridTemplates)
            {
                GridTemplate reflection = GridTemplate.Reflect(gridTemplate, Reflection.R1);
                if (!extendedGridTemplates.Contains(reflection)) extendedGridTemplates.Add(reflection);
                reflection = GridTemplate.Reflect(gridTemplate, Reflection.R2);
                if (!extendedGridTemplates.Contains(reflection)) extendedGridTemplates.Add(reflection);
                reflection = GridTemplate.Reflect(gridTemplate, Reflection.R3);
                if (!extendedGridTemplates.Contains(reflection)) extendedGridTemplates.Add(reflection);
            }

            return extendedGridTemplates;
        }


        public bool Matches(int[][][] coordinateDistanceGrid, int startX, int startY, int startZ)
        {
            bool shouldTestX = false;
            bool passedXTest = false;

            for (int x = 0; x < pointMask.Length; x++)
            {
                for (int y = 0; y < pointMask.Length; y++)
                {
                    for (int z = 0; z < pointMask.Length; z++)
                    {
                        if (pointMask[x][y][z] == PointType.IGNORED) continue;

                        if (coordinateDistanceGrid[startX-1+x][startY-1+y][startZ-1+z] != 0 /*is Black*/)
                        {
                            if (pointMask[x][y][z] == PointType.WHITE) return false;
                            if (pointMask[x][y][z] == PointType.X) passedXTest = true;
                        }
                        else /*is White*/
                        {
                            if (pointMask[x][y][z] == PointType.BLACK) return false;
                            if (pointMask[x][y][z] == PointType.X) shouldTestX = true;
                        }
                    }
                }
            }

            return !shouldTestX || passedXTest;
        }

        public bool Matches(VoxelGrid voxelGrid, int startX, int startY, int startZ)
        {
            bool shouldTestX = false;
            bool passedXTest = false;

            for (int x = 0; x <= 2; x++)
            {
                for (int y = 0; y <= 2; y++)
                {
                    for (int z = 0; z <= 2; z++)
                    {
                        if (voxelGrid.coordinateGrid[startX-1+x][startY-1+y][startZ-1+z] /*is Black*/)
                        {
                            if (pointMask[x][y][z] == PointType.WHITE) return false;

                            if (pointMask[x][y][z] == PointType.X) passedXTest = true;
                        }
                        else /*is White*/
                        {
                            if (pointMask[x][y][z] == PointType.BLACK) return false;

                            if (pointMask[x][y][z] == PointType.X) shouldTestX = true;
                        }
                    }
                }
            }
            return !shouldTestX || passedXTest;
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                GridTemplate other = (GridTemplate) obj;

                for (int x = 0; x < pointMask.Length; x++)
                {
                    for (int y = 0; y < pointMask[0].Length; y++)
                    {
                        for (int z = 0; z < pointMask[0][0].Length; z++)
                        {
                            if (pointMask[x][y][z] != other.pointMask[x][y][z])
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
        }

        public override int GetHashCode()
        {
            return this.pointMask.GetHashCode() ^ this.pointMask.Length.GetHashCode();
        }
    }
}