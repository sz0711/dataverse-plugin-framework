using Microsoft.Xrm.Sdk;
using System;

namespace PluginInfrastructure.Rules
{
    /// <summary>
    /// Generic rule engine for evaluating business rules against entities.
    /// Demonstrates delegate-based pattern for flexible, composable rule evaluation.
    /// </summary>
    public class RuleEngine<T> where T : Entity
    {
        /// <summary>
        /// Delegate definition for a rule that evaluates an entity.
        /// Returns true if the rule condition is met.
        /// </summary>
        public delegate bool Rule(T entity);

        /// <summary>
        /// Evaluate a single rule against an entity.
        /// </summary>
        /// <param name="entity">Entity to evaluate</param>
        /// <param name="rule">Rule to apply</param>
        /// <returns>True if rule condition is met</returns>
        public bool Evaluate(T entity, Rule rule)
        {
            return rule?.Invoke(entity) ?? false;
        }

        /// <summary>
        /// Evaluate all rules against an entity (AND logic - all must pass).
        /// </summary>
        /// <param name="entity">Entity to evaluate</param>
        /// <param name="rules">Rules to apply</param>
        /// <returns>True if all rules pass</returns>
        public bool EvaluateAll(T entity, params Rule[] rules)
        {
            if (rules == null || rules.Length == 0)
                return true;

            foreach (var rule in rules)
            {
                if (!Evaluate(entity, rule))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Evaluate any rule against an entity (OR logic - at least one must pass).
        /// </summary>
        /// <param name="entity">Entity to evaluate</param>
        /// <param name="rules">Rules to apply</param>
        /// <returns>True if any rule passes</returns>
        public bool EvaluateAny(T entity, params Rule[] rules)
        {
            if (rules == null || rules.Length == 0)
                return false;

            foreach (var rule in rules)
            {
                if (Evaluate(entity, rule))
                    return true;
            }

            return false;
        }
    }
}
