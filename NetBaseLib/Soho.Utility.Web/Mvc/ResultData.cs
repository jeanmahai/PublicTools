﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soho.Utility.Web.Mvc
{
    public class ResultData<T>
    {       
        public List<T> Data { get; set; }
        public int TotalCount { get; set; }
    }
}