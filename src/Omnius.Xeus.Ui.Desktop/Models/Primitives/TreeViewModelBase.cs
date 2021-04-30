using System.Collections.Generic;
using Omnius.Core;

namespace Omnius.Xeus.Ui.Desktop.Models.Primitives
{
    public abstract class TreeViewModelBase : DisposableBase, IDropable
    {
        public TreeViewModelBase(TreeViewModelBase? parent)
        {
            this.Parent = parent;
        }

        public TreeViewModelBase? Parent { get; private set; }

        public IEnumerable<TreeViewModelBase> GetAncestors()
        {
            var list = new LinkedList<TreeViewModelBase>();
            list.AddFirst(this);

            for (; ; )
            {
                var parent = list.First?.Value?.Parent;
                if (parent == null) break;

                list.AddFirst(parent);
            }

            return list;
        }

        public abstract bool TryAdd(object value);

        public abstract bool TryRemove(object value);
    }
}
