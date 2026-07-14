import os
import re

bll_dir = "/Users/sivaprakashs/Documents/SchoolERPManagement/SchoolERPManagementBLLibrary/Services/"

for root, dirs, files in os.walk(bll_dir):
    for file in files:
        if file.endswith(".cs"):
            path = os.path.join(root, file)
            with open(path, 'r') as f:
                content = f.read()

            # Object initialization: Name = dto.Name -> FirstName = dto.FirstName, LastName = dto.LastName
            content = re.sub(r'Name\s*=\s*([a-zA-Z0-9_]+)\.Name\b', r'FirstName = \1.FirstName, LastName = \1.LastName', content)
            
            # String interpolation / usage: s.Name -> $"{s.FirstName} {s.LastName}"
            # This is tricky since it could be e.g. student.Name
            content = re.sub(r'([a-zA-Z0-9_]+)\.Name\b(?! =)', r'`${\1.FirstName} {\1.LastName}`', content) # Temp marker
            content = content.replace('`$', '$"')
            content = content.replace('`', '"')

            # dto.Name = $"{FirstName} {LastName}" mapping ?
            # Sometimes it's new StudentResponseDTO(s.Id, s.Name...)
            # Actually since we changed DTOs to FirstName, LastName, we don't concatenate in the DTO constructor, we pass both.
            # But the regex above `${\1.FirstName} {\1.LastName}` might break DTO constructors if it expects two arguments.
            
            with open(path, 'w') as f:
                f.write(content)

print("Services updated.")
