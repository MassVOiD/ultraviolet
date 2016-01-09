﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Uvss.Testing
{
    /// <summary>
    /// Represents a unit test result containing a syntax node.
    /// </summary>
    /// <typeparam name="TNode">The syntax node being examined.</typeparam>
    public sealed class SyntaxNodeResult<TNode> where TNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxNodeResult{TNode}"/> class.
        /// </summary>
        /// <param name="node">The node being examined.</param>
        internal SyntaxNodeResult(TNode node)
        {
            this.node = node;
        }

        /// <summary>
        /// Asserts that the node satisfies the specified condition.
        /// </summary>
        /// <param name="condition">The condition against which to evaluate the node.</param>
        /// <returns>The result object.</returns>
        public SyntaxNodeResult<TNode> ShouldSatisfyTheCondition(Func<TNode, Boolean> condition)
        {
            Assert.IsTrue(condition(node));
            return this;
        }

        /// <summary>
        /// Asserts that this object is of the specified type.
        /// </summary>
        /// <typeparam name="TType">The expected type.</typeparam>
        /// <returns>The result object.</returns>
        public SyntaxNodeResult<TType> ShouldBeOfType<TType>()
            where TType : SyntaxNode
        {
            Assert.IsInstanceOfType(node, typeof(TType));
            return new SyntaxNodeResult<TType>(node as TType);
        }

        /// <summary>
        /// Asserts that this object should be null.
        /// </summary>
        /// <returns>The result object.</returns>
        public SyntaxNodeResult<TNode> ShouldBeNull()
        {
            Assert.IsNull(node);
            return this;
        }

        /// <summary>
        /// Asserts that this object should not be null.
        /// </summary>
        /// <returns>The result object.</returns>
        public SyntaxNodeResult<TNode> ShouldNotBeNull()
        {
            Assert.IsNotNull(node);
            return this;
        }

        /// <summary>
        /// Asserts that this node should be non-null and flagged as missing.
        /// </summary>
        /// <returns>The result object.</returns>
        public SyntaxNodeResult<TNode> ShouldBeMissing()
        {
            Assert.IsNotNull(node);
            Assert.IsTrue(node.IsMissing, "Node is not missing.");
            return this;
        }

        /// <summary>
        /// Asserts that this node should be non-null and not flagged as missing.
        /// </summary>
        /// <returns>The result object.</returns>
        public SyntaxNodeResult<TNode> ShouldBePresent()
        {
            Assert.IsNotNull(node);
            Assert.IsFalse(node.IsMissing, "Node is missing.");
            return this;
        }

        /// <summary>
        /// Asserts that this node should have the specified text, ignoring any leading or trailing trivia.
        /// </summary>
        /// <param name="expected">The expected text.</param>
        /// <param name="includeTrivia">A value indicating whether to include trivia.</param>
        /// <returns>The result object.</returns>
        public SyntaxNodeResult<TNode> ShouldHaveFullString(String expected, Boolean includeTrivia = false)
        {
            if (includeTrivia)
            {
                Assert.AreEqual(expected, node.ToFullString());
                return this;
            }

            var triviaWidthLeading = node.GetLeadingTriviaWidth();
            var triviaWidthTrailing = node.GetTrailingTriviaWidth();

            var actual = node.ToFullString();
            var actualWithoutTrivia = actual.Substring(triviaWidthLeading,
                actual.Length - (triviaWidthLeading + triviaWidthTrailing));

            Assert.AreEqual(expected, actualWithoutTrivia);
            return this;
        }

        /// <summary>
        /// Asserts that this node has the specified kind.
        /// </summary>
        /// <param name="expected">The expected kind.</param>
        /// <returns>The result object.</returns>
        public SyntaxNodeResult<TNode> ShouldBeOfKind(SyntaxKind expected)
        {
            Assert.AreEqual(expected, node.Kind);
            return this;
        }

        /// <summary>
        /// Gets the underlying node.
        /// </summary>
        public TNode Node
        {
            get { return node; }
        }

        // Property values.
        private readonly TNode node;
    }
}
