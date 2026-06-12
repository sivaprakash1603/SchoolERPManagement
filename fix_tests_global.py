import os
import glob
import re

test_dir = '/Users/sivaprakashs/Documents/SchoolERPManagement/SchoolERPManagement.Tests'

for root, _, files in os.walk(test_dir):
    for file_name in files:
        if file_name.endswith('Tests.cs'):
            file_path = os.path.join(root, file_name)
            with open(file_path, 'r') as f:
                content = f.read()
            
            # AutoMapper fix
            content = content.replace('new Moq.Mock<AutoMapper.IMapper>().Object', 'SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()')
            
            # MockQueryable fix
            content = content.replace('.AsQueryable().BuildMock()', '.BuildMockDbSet().Object')
            content = content.replace('.BuildMock()', '.BuildMockDbSet().Object')
            
            with open(file_path, 'w') as f:
                f.write(content)

