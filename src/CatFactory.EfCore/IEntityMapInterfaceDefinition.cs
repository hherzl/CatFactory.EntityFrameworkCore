﻿using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class IEntityMapInterfaceDefinition : CSharpInterfaceDefinition
    {
        public IEntityMapInterfaceDefinition()
        {
            Namespaces.Add("Microsoft.EntityFrameworkCore");

            Name = "IEntityMap";

            Methods.Add(new MethodDefinition("void", "Map")
            {
                Parameters = new List<ParameterDefinition>()
                {
                    new ParameterDefinition("ModelBuilder", "modelBuilder")
                }
            });
        }
    }
}