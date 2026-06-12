import os
import glob

test_dir = '/Users/sivaprakashs/Documents/SchoolERPManagement/SchoolERPManagement.Tests/Services'

for file_path in glob.glob(os.path.join(test_dir, '*Tests.cs')):
    with open(file_path, 'r') as f:
        content = f.read()
    
    # change back to BuildMock
    content = content.replace('.BuildMockDbSet()', '.BuildMock()')
    
    # Ensure using MockQueryable.EntityFrameworkCore; is present
    if 'using MockQueryable.EntityFrameworkCore;' not in content:
        content = 'using MockQueryable.EntityFrameworkCore;\n' + content
        
    with open(file_path, 'w') as f:
        f.write(content)

