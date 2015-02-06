using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace De.DataExperts.conhITApp
{
    class SkeletData
    {
        
        public ColorSpacePoint HeartPosition { get; set; }
        public float HeartDistance { get; set; }
        public float heartWidth { get; set; }

        public ColorSpacePoint HeadPosition { get; set; }

        public double HeadDistance { get; set; }
    }
}
