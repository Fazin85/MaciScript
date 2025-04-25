namespace MaciScript
{
    public class MaciNamedScopeVariableAllocator
    {
        private readonly Dictionary<string, Scope> scopes = [];

        private class Scope
        {
            public List<Variable> Variables { get; } = [];
        }

        private class Variable
        {
            public required string Name;
            public required int Value;
        }

        public void AllocateScope(string name)
        {
            if (scopes.ContainsKey(name))
            {
                throw new Exception("Cannot have multiple scopes with the same name: " + name);
            }

            scopes[name] = new();
        }

        public void AllocateVariable(string scopeName, string variableName)
        {
            if (!scopes.TryGetValue(scopeName, out Scope? scope))
            {
                throw new Exception("No scope with name: " + scopeName);
            }

            if (scope.Variables.Any(x => x.Name == variableName))
            {
                throw new Exception("Cannot have multiple variables with the same name in a scope: " + variableName);
            }

            scope.Variables.Add
            (
                new()
                {
                    Name = variableName,
                    Value = 0
                }
            );
        }

        public int GetVariable(string scopeName, string variableName)
        {
            if (!scopes.TryGetValue(scopeName, out Scope? scope))
            {
                throw new Exception("No scope with name: " + scopeName);
            }
            var variable = scope.Variables.Find(x => x.Name == variableName);

            return variable == null ? throw new Exception("No variable with name: " + variableName + " in scope: " + scopeName) : variable.Value;
        }

        public void SetVariable(string scopeName, string variableName, int value)
        {
            if (!scopes.TryGetValue(scopeName, out Scope? scope))
            {
                throw new Exception("No scope with name: " + scopeName);
            }
            var variable = scope.Variables.Find(x => x.Name == variableName) ?? throw new Exception("No variable with name: " + variableName + " in scope: " + scopeName);
            variable.Value = value;
        }

        public void FreeScope(string name)
        {
            if (!scopes.ContainsKey(name))
            {
                throw new Exception("No scope with name: " + name);
            }

            scopes.Remove(name);
        }
    }
}
