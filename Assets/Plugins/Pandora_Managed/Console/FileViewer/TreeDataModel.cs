using System.Collections.Generic;

namespace com.tencent.pandora
{
    /// <summary>
    /// ����Ԫ��
    /// </summary>
    public class TreeElement
    {
        public string Name { get; set; }
        public int Depth { get; set; }
        public TreeElement Parent { get; set; }
        public List<TreeElement> Children { get; set; }

        public bool HasChildren
        {
            get
            {
                return Children != null && Children.Count > 0;
            }
        }
    }

    /// <summary>
    /// ���νṹ��������
    /// </summary>
    public class TreeUtility
    {
        public static T ListToTree<T>(List<T> list) where T : TreeElement
        {
            bool depthValid = ValidateDepthValues<T>(list);
            if (depthValid == false)
            {
                return null;
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i].Parent = null;
                list[i].Children = null;
            }

            for (int i = 0; i < list.Count; i++)
            {
                TreeElement parent = list[i];
                int parentDepth = parent.Depth;
                List<TreeElement> childrenList = new List<TreeElement>();
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[j].Depth <= parentDepth)
                    {
                        break;
                    }
                    if (list[j].Depth == parentDepth + 1)
                    {
                        list[j].Parent = parent;
                        childrenList.Add(list[j]);
                    }
                }
                if (childrenList.Count > 0)
                {
                    parent.Children = childrenList;
                }
            }

            return list[0];
        }

        public static void TreeToList<T>(T root, List<T> result) where T : TreeElement
        {
            if (result == null)
            {
                Logger.LogError("list Ϊnull������");
                return;
            }

            result.Clear();
            Stack<T> stack = new Stack<T>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                T item = stack.Pop();
                result.Add(item);

                List<TreeElement> children = item.Children;
                if (children == null)
                {
                    continue;
                }

                for (int i = children.Count - 1; i >= 0; i--)
                {
                    stack.Push((T)children[i]);
                }
            }
        }

        private static bool ValidateDepthValues<T>(List<T> list) where T : TreeElement
        {
            if (list == null || list.Count == 0)
            {
                Logger.LogError("list Ϊnull��գ�����");
                return false;
            }

            if (list[0].Depth != 0)
            {
                Logger.LogError("���ڵ���ȱ���Ϊ0�������б�ĸ��ڵ���Ȳ��������������飡");
                return false;
            }

            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].Depth <= 0)
                {
                    Logger.LogError(string.Format("�����ڵ��⣬���нڵ���ȱ������0����ǰ��{0}���ڵ����С��0�����飡", i));
                    return false;
                }
            }

            for (int i = 0; i < list.Count - 1; i++)
            {
                int currentElementDepth = list[i].Depth;
                int nextElementDepth = list[i + 1].Depth;
                if (currentElementDepth < nextElementDepth && currentElementDepth + 1 < nextElementDepth)
                {
                    Logger.LogError(string.Format("���ڽڵ�depth����ֻ������1����ǰ��{0}���ڵ�������{1},���飡", i + 1, nextElementDepth - currentElementDepth));
                    return false;
                }
            }

            return true;
        }
    }
}