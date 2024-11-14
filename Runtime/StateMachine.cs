using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkMoon.GameFramework.FSM
{
    public class StateMachine
    {
        #region 属性
        private readonly Dictionary<string, System.Object> m_BlackBoard;
        private readonly Dictionary<string, IStateNode> m_Nodes;

        private IStateNode m_CurNode;
        private IStateNode m_PreNode;

        /// <summary>
        /// 状态机持有者
        /// </summary>
        public System.Object Owner { get; private set; }

        /// <summary>
        /// 当前运行节点名字
        /// </summary>
        public string CurrentNode
        {
            get
            {
                return m_CurNode != null ? m_CurNode.GetType().FullName : string.Empty;
            }
        }

        /// <summary>
        /// 之前运行节点名字
        /// </summary>
        public string PrevioysNode
        {
            get
            {
                return m_PreNode != null ? m_PreNode.GetType().FullName : string.Empty;
            }
        }
        #endregion

        #region 构造
        private StateMachine()
        {

        }

        public StateMachine(System.Object owner, int DefaultBlackBoardCount = 100, int DefaultNodesCount = 100)
        {
            m_BlackBoard = new Dictionary<string, System.Object>(DefaultBlackBoardCount);
            m_Nodes = new Dictionary<string, IStateNode>(DefaultNodesCount);
            Owner = owner;
        }
        #endregion

        #region 更新状态机
        public void Update()
        {
            m_CurNode?.OnUpdate();
        }
        #endregion

        #region 启动状态机
        public void Run<T>() where T : IStateNode
        {
            var nodeType = typeof(T);
            var nodeName = nodeType.FullName;
            Run(nodeName);
        }

        public void Run(Type entryNode)
        {
            Run(entryNode.FullName);
        }

        public void Run(string entryNode)
        {
            m_CurNode = _TryGetNode(entryNode);
            m_PreNode = m_CurNode;

            if (m_CurNode == null)
            {
                throw new Exception($"Not found node: {entryNode}");
            }

            m_CurNode.OnEnter();
        }
        #endregion

        #region 添加一个状态
        public void AddNode<T>() where T : IStateNode
        {
            var nodeType = typeof(T);
            var stateNode = Activator.CreateInstance(nodeType) as IStateNode;
            AddNode(stateNode);
        }

        public void AddNode(IStateNode stateNode)
        {
            if (stateNode == null)
            {
                throw new ArgumentNullException();
            }

            var nodeType = stateNode.GetType();
            var nodeName = nodeType.FullName;

            if (!m_Nodes.ContainsKey(nodeName))
            {
                stateNode.OnCreate(this);
                m_Nodes.Add(nodeName, stateNode);
            }
            else
            {
                Debug.LogError($"State node already existed : {nodeName}");
            }
        }
        #endregion

        #region 切换状态
        public void ChangeState<T>() where T : IStateNode
        {
            var nodeType = typeof(T);
            var nodeName = nodeType.FullName;
            ChangeState(nodeName);
        }

        public void ChangeState(Type nodeType)
        {
            ChangeState(nodeType?.FullName);
        }

        public void ChangeState(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
                throw new ArgumentNullException();

            IStateNode node = _TryGetNode(nodeName);
            if (node == null)
            {
                Debug.LogError($"Can not found state node : {nodeName}");
                return;
            }

            Debug.Log($"{m_CurNode.GetType().FullName} --> {node.GetType().FullName}");
            m_PreNode = m_CurNode;
            m_CurNode.OnExit();
            m_CurNode = node;
            m_CurNode.OnEnter();
        }
        #endregion

        #region 黑板数据
        public void SetBlackboardValue(string key, System.Object value)
        {
            if (m_BlackBoard.ContainsKey(key))
            {
                m_BlackBoard[key] = value;
            }
            else
            {
                m_BlackBoard.Add(key, value);
            }
        }

        public System.Object GetBlackboardValue(string key)
        {
            if (m_BlackBoard.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }
        #endregion

        private IStateNode _TryGetNode(string nodeName)
        {
            m_Nodes.TryGetValue(nodeName, out var node);
            return node;
        }
    }
}