using System;
using System.Runtime.Serialization;

namespace Soho.Utility.DataAccess.SearchEngine
{
    public class Expression
    {
        #region Field

        private Expression leftNode;
        private Expression rightNode;
        private FilterBase nodeData;
        private Operation operation;

        #endregion Field

        #region Property

        /// <summary>
        /// 左表达式
        /// </summary>
        public Expression LeftNode
        {
            get { return leftNode; }
        }

        /// <summary>
        /// 右表达式
        /// </summary>
        public Expression RightNode
        {
            get { return rightNode; }
        }

        /// <summary>
        /// 节点值
        /// </summary>
        public FilterBase NodeData
        {
            get { return nodeData; }
        }

        /// <summary>
        /// 运算符
        /// </summary>
        public Operation Operation
        {
            get { return operation; }
        }

        #endregion Property

        #region Constructor

        public Expression()
        {
        }

        public Expression(FilterBase data)
        {
            nodeData = data;
            operation = Operation.None;
        }

        public Expression(FilterBase data, Operation op)
        {
            if (op != Operation.NOT && op != Operation.None)
            {
                throw new Exception("运算符错误！叶节点只能使用单目运算符。");
            }
            nodeData = data;
            operation = op;
        }

        public Expression(Expression left, Expression right, Operation op)
        {
            if (op != Operation.OR && op != Operation.AND)
            {
                throw new Exception("运算符错误！此处只能使用双目运算符");
            }
            leftNode = left;
            rightNode = right;
            operation = op;
        }

        public Expression(FilterBase leftData, Expression right, Operation op)
        {
            if (op != Operation.OR && op != Operation.AND)
            {
                throw new Exception("运算符错误！此处只能使用双目运算符");
            }
            leftNode = new Expression(leftData);
            rightNode = right;
            operation = op;
        }

        public Expression(Expression left, FilterBase rightData, Operation op)
        {
            if (op != Operation.OR && op != Operation.AND)
            {
                throw new Exception("运算符错误！此处只能使用双目运算符");
            }
            leftNode = left;
            rightNode = new Expression(rightData);
            operation = op;
        }

        public Expression(FilterBase leftData, FilterBase rightData, Operation op)
        {
            if (op != Operation.OR && op != Operation.AND)
            {
                throw new Exception("运算符错误！此处只能使用双目运算符");
            }
            leftNode = new Expression(leftData);
            rightNode = new Expression(rightData);
            operation = op;
        }

        #endregion Constructor

        #region Public Method

        public bool HasChild()
        {
            return leftNode != null || rightNode != null;
        }

        public bool IsEmpty()
        {
            return leftNode == null && rightNode == null && nodeData == null;
        }

        #endregion Public Method
    }
}