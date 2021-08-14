using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace JunimoStudio.Menus.Controls
{
    public abstract class Container : Element
    {
        private readonly IList<Element> ChildrenImpl = new List<Element>();

        public Element RenderLast { get; set; }

        public Element[] Children => ChildrenImpl.ToArray();

        public void AddChild(Element element)
        {
            element.Parent?.RemoveChild(element);
            ChildrenImpl.Add(element);
            element.Parent = this;
        }

        public void RemoveChild(Element element)
        {
            if (element.Parent != this)
                throw new ArgumentException("Element must be a child of this container.");
            ChildrenImpl.Remove(element);
            element.Parent = null;
        }

        public override void Draw(SpriteBatch b)
        {
            foreach (var child in ChildrenImpl)
            {
                if (child == RenderLast)
                    continue;
                child.Draw(b);
            }
            RenderLast?.Draw(b);
        }
    }
}
