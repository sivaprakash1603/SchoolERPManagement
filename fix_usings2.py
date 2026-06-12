import os
import glob

test_dir = '/Users/sivaprakashs/Documents/SchoolERPManagement/SchoolERPManagement.Tests/Services'

for file_path in glob.glob(os.path.join(test_dir, '*Tests.cs')):
    with open(file_path, 'r') as f:
        content = f.read()
    
    content = content.replace('using MockQueryable.EntityFrameworkCore;', 'using MockQueryable;')
    
    with open(file_path, 'w') as f:
        f.write(content)

