﻿using Structurizr.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Structurizr.Analysis
{

    /// <summary>
    /// This is the base class for component finder strategies that use the standard .NET assembly
    /// scanning facility to find components.
    /// </summary>
    public class AssemblyScanningComponentFinderStrategy : ComponentFinderStrategy
    {

        private List<ITypeMatcher> typeMatchers = new List<ITypeMatcher>();

        public AssemblyScanningComponentFinderStrategy(params ITypeMatcher[] typeMatchers)
        {
            this.typeMatchers.AddRange(typeMatchers);
        }

        public override ICollection<Component> FindComponents()
        {
            List<Component> components = new List<Component>();

            IEnumerable<Type> types =   from a in AppDomain.CurrentDomain.GetAssemblies()
                                        from t in a.GetTypes()
                                        where InNamespace(t)
                                        select t;

            foreach (Type type in types)
            {
                foreach (ITypeMatcher typeMatcher in this.typeMatchers)
                {
                    if (typeMatcher.Matches(type))
                    {
                        if (type.IsInterface)
                        {
                            components.Add(this.ComponentFinder.FoundComponent(type.Name, type.AssemblyQualifiedName, typeMatcher.GetDescription(), typeMatcher.GetTechnology(), null));
                        }
                        else
                        {
                            components.Add(this.ComponentFinder.FoundComponent(type.Name, type.AssemblyQualifiedName, typeMatcher.GetDescription(), typeMatcher.GetTechnology(), null));
                        }
                    }
                }
            }

            return components;
        }

        public override void FindDependencies()
        {
            foreach (Component component in ComponentFinder.Container.Components)
            {
                if (component.Type != null)
                {
                    Type type = Type.GetType(component.Type);
                    foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        string propertyType = propertyInfo.PropertyType.AssemblyQualifiedName;
                        Component c = ComponentFinder.Container.GetComponentOfType(propertyType);
                        if (c != null)
                        {
                            component.Uses(c, null);
                        }
                    }
                    foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        string fieldType = fieldInfo.FieldType.AssemblyQualifiedName;
                        Component c = ComponentFinder.Container.GetComponentOfType(fieldType);
                        if (c != null)
                        {
                            component.Uses(c, "");
                        }
                    }
                }
            }
        }

        private bool InNamespace(Type type)
        {
            return type.Namespace != null && type.Namespace.StartsWith(ComponentFinder.Namespace);
        }

    }
}
