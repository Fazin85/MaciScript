namespace MaciScript
{
    public class MaciStackVariableAllocator
    {
        private readonly Stack<Scope> scopes = [];
        private class Scope
        {
            public List<Variable> Variables { get; } = [];
        }
        private class Variable
        {
            public required string Name;
            public required int Value;
        }

        public void PushScope()
        {
            scopes.Push(new());
        }

        public void AllocateVariable(string name)
        {
            if (scopes.Count == 0)
            {
                throw new Exception("Tried to allocate stack variable, but no scope currently exists");
            }
            Scope scope = scopes.Peek();
            if (scope.Variables.Any(x => x.Name == name))
            {
                throw new Exception("Cannot have multiple variables with the same name in a scope: " + name);
            }
            scope.Variables.Add(
                new()
                {
                    Name = name,
                    Value = 0
                }
            );
        }

        public int GetVariable(string name)
        {
            if (scopes.Count == 0)
            {
                throw new Exception("Tried to get stack variable, but no scope currently exists");
            }

            foreach (var scope in scopes)
            {
                var variable = scope.Variables.Find(x => x.Name == name);
                if (variable != null)
                {
                    return variable.Value;
                }
            }

            throw new Exception($"No variable with name: {name} found in any accessible scope");
        }

        public void SetVariable(string name, int value)
        {
            if (scopes.Count == 0)
            {
                throw new Exception("Tried to set stack variable, but no scope currently exists");
            }

            foreach (var scope in scopes)
            {
                var variable = scope.Variables.Find(x => x.Name == name);
                if (variable != null)
                {
                    variable.Value = value;
                    return;
                }
            }

            throw new Exception($"No variable with name: {name} found in any accessible scope");
        }

        public void PopScope()
        {
            if (scopes.Count == 0)
            {
                throw new Exception("Cannot pop scope, no scope currently exists");
            }
            scopes.Pop();
        }
    }
}