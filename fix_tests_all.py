import os
import glob
import re

test_dir = '/Users/sivaprakashs/Documents/SchoolERPManagement/SchoolERPManagement.Tests/Services'

for file_path in glob.glob(os.path.join(test_dir, '*Tests.cs')):
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Fix 1: AutoMapper
    content = content.replace('new Moq.Mock<AutoMapper.IMapper>().Object', 'SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()')
    
    # Fix 2: MockQueryable
    # We replace .AsQueryable().BuildMock() with .BuildMockDbSet().Object
    content = content.replace('.AsQueryable().BuildMock()', '.BuildMockDbSet().Object')
    content = content.replace('.BuildMock()', '.BuildMockDbSet().Object')  # catch any missed ones
    # Careful, we might have .BuildMockDbSet().Object.Object if we double replace, but we only have .AsQueryable().BuildMock()
    # Let's use regex to be safe
    
    with open(file_path, 'w') as f:
        f.write(content)

# Update csproj
csproj = '/Users/sivaprakashs/Documents/SchoolERPManagement/SchoolERPManagement.Tests/SchoolERPManagement.Tests.csproj'
with open(csproj, 'r') as f:
    content = f.read()
    
content = re.sub(r'PackageReference Include="MockQueryable\.Moq" Version=".*?"', 'PackageReference Include="MockQueryable.Moq" Version="8.0.1"', content)

with open(csproj, 'w') as f:
    f.write(content)

print("Done")
