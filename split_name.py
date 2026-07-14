import os
import re

models = ["Student.cs", "Teacher.cs", "Parent.cs", "Staff.cs"]
base_dir = "/Users/sivaprakashs/Documents/SchoolERPManagement/SchoolERPManagementModelLibrary/Models/"

for model in models:
    path = os.path.join(base_dir, model)
    with open(path, 'r') as f:
        content = f.read()
    
    content = re.sub(r'public string Name { get; set; } = null!;', 'public string FirstName { get; set; } = null!;\n    public string LastName { get; set; } = null!;', content)
    
    with open(path, 'w') as f:
        f.write(content)

print("Models updated.")
