﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragAndDropMVVM.Model
{
    public interface IDiagramLayout
    {
        double X { get; set; }
        double Y { get; set; }
        Type DiagramType { get; set; }
        string DiagramUUID { get; set; }
        ILineLayout[] DepartureLines { get; }

        object DataContext { get; set; }
    }
}