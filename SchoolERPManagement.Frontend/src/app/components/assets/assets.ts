import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AssetService, AssetResponseDTO, AssetTypeResponseDTO, CreateAssetDTO, AssetIssueDTO, AssetReportResponseDTO } from '../../services/asset.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { StudentService } from '../../services/student.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-assets',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './assets.html',
  styleUrl: './assets.css'
})
export class Assets implements OnInit {
  private assetService = inject(AssetService);
  private classService = inject(ClassService);
  private studentService = inject(StudentService);
  private toastService = inject(ToastService);

  // Auth Info
  userRole = signal<string>('Student');
  studentClassId = signal<number | null>(null);

  // Lists
  assets = signal<AssetResponseDTO[]>([]);
  assetTypes = signal<AssetTypeResponseDTO[]>([]);
  classes = signal<ClassResponseDTO[]>([]);

  // Filter signals
  searchQuery = signal<string>('');
  selectedTypeId = signal<number | null>(null);
  selectedStatus = signal<'all' | 'active' | 'under repair' | 'broken'>('all');

  // Stats
  totalAssetsCount = signal<number>(0);
  activeAssetsCount = signal<number>(0);
  underRepairAssetsCount = signal<number>(0);
  brokenAssetsCount = signal<number>(0);

  // Modals visibility
  showAddAssetModal = signal<boolean>(false);
  showReportIssueModal = signal<boolean>(false);
  showResolveModal = signal<boolean>(false);

  // Lists
  assetReports = signal<AssetReportResponseDTO[]>([]);

  // Forms state
  newAsset = signal<CreateAssetDTO>({
    assetname: '',
    assettypeId: undefined,
    purchasedate: '',
    warrantyexpiry: '',
    status: 'active',
    assignedClassId: undefined
  });

  reportIssue = signal<AssetIssueDTO>({
    assetId: 0,
    status: 'under repair',
    report: ''
  });

  resolveForm = signal<AssetIssueDTO>({
    assetId: 0,
    status: 'active',
    report: ''
  });

  // Loading indicator
  isLoading = signal<boolean>(false);

  // Selected Asset Name for Report/Resolve Modals
  selectedAssetNameForReport = signal<string>('');
  selectedAssetNameForResolve = signal<string>('');

  ngOnInit() {
    const role = sessionStorage.getItem('role') || 'Student';
    this.userRole.set(role);

    const userIdStr = sessionStorage.getItem('userId');
    const userId = userIdStr ? parseInt(userIdStr, 10) : null;

    if (role === 'Student' && userId) {
      this.isLoading.set(true);
      this.studentService.getStudentByUserId(userId).subscribe({
        next: (student) => {
          this.studentClassId.set(student.classId);
          this.loadInitialData();
        },
        error: (err) => {
          console.error('Failed to resolve student profile', err);
          this.loadInitialData();
        }
      });
    } else {
      this.loadInitialData();
    }
  }

  loadInitialData() {
    this.isLoading.set(true);
    
    // Fetch all required lists
    this.assetService.getAssets().subscribe({
      next: (assetsData) => {
        // If student, filter assets by class
        if (this.userRole() === 'Student' && this.studentClassId()) {
          const classAssets = assetsData.filter(a => a.assignedClassId === this.studentClassId());
          this.assets.set(classAssets);
        } else {
          this.assets.set(assetsData);
        }
        this.calculateStats();
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to load assets list.');
      }
    });

    this.assetService.getAssetTypes().subscribe({
      next: (types) => this.assetTypes.set(types),
      error: (err) => console.error('Failed to load asset types', err)
    });

    this.classService.getAllClasses().subscribe({
      next: (classesData) => this.classes.set(classesData),
      error: (err) => console.error('Failed to load classes', err)
    });

    if (this.userRole() === 'Admin') {
      this.loadReports();
    }

    this.isLoading.set(false);
  }

  loadReports() {
    this.assetService.getAssetReports().subscribe({
      next: (reports) => {
        this.assetReports.set(reports.sort((a, b) => new Date(b.createdat || '').getTime() - new Date(a.createdat || '').getTime()));
      },
      error: (err) => console.error('Failed to load reports', err)
    });
  }

  calculateStats() {
    const list = this.assets();
    this.totalAssetsCount.set(list.length);
    this.activeAssetsCount.set(list.filter(a => a.status?.toLowerCase() === 'active').length);
    this.underRepairAssetsCount.set(list.filter(a => a.status?.toLowerCase() === 'under repair' || a.status?.toLowerCase() === 'under_repair').length);
    this.brokenAssetsCount.set(list.filter(a => a.status?.toLowerCase() === 'broken').length);
  }

  getFilteredAssets(): AssetResponseDTO[] {
    const query = this.searchQuery().toLowerCase().trim();
    const typeId = this.selectedTypeId();
    const status = this.selectedStatus();

    return this.assets().filter(a => {
      const matchesSearch = a.assetname.toLowerCase().includes(query);
      const matchesType = typeId === null || a.assettypeId === typeId;
      
      let matchesStatus = true;
      if (status !== 'all') {
        const cleanStatus = a.status?.toLowerCase().replace('_', ' ') || '';
        matchesStatus = cleanStatus === status;
      }

      return matchesSearch && matchesType && matchesStatus;
    });
  }

  getTypeName(typeId?: number): string {
    if (!typeId) return 'N/A';
    const found = this.assetTypes().find(t => t.id === typeId);
    return found ? found.typename : `Type #${typeId}`;
  }

  getClassName(classId?: number): string {
    if (!classId) return 'N/A';
    const found = this.classes().find(c => c.id === classId);
    return found ? `${found.classname} - ${found.section || ''}` : `Class #${classId}`;
  }

  openAddAssetModal() {
    this.newAsset.set({
      assetname: '',
      assettypeId: undefined,
      purchasedate: '',
      warrantyexpiry: '',
      status: 'active',
      assignedClassId: undefined
    });
    this.showAddAssetModal.set(true);
  }

  closeAddAssetModal() {
    this.showAddAssetModal.set(false);
  }

  saveAsset() {
    const dto = this.newAsset();
    if (!dto.assetname.trim()) {
      this.toastService.warning('Asset name is required.');
      return;
    }

    this.isLoading.set(true);
    this.assetService.addAsset(dto).subscribe({
      next: (savedAsset) => {
        this.toastService.success('Asset added successfully!');
        this.assets.update(list => [savedAsset, ...list]);
        this.calculateStats();
        this.closeAddAssetModal();
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to add asset.');
        this.isLoading.set(false);
      }
    });
  }

  openReportIssueModal(asset: AssetResponseDTO) {
    this.selectedAssetNameForReport.set(asset.assetname);
    this.reportIssue.set({
      assetId: asset.id,
      status: 'under repair',
      report: ''
    });
    this.showReportIssueModal.set(true);
  }

  closeReportIssueModal() {
    this.showReportIssueModal.set(false);
  }

  submitIssueReport() {
    const dto = this.reportIssue();
    if (!dto.report?.trim()) {
      this.toastService.warning('Please provide an issue description.');
      return;
    }

    this.isLoading.set(true);
    this.assetService.reportAssetIssue(dto).subscribe({
      next: (reportResult) => {
        this.toastService.success('Issue reported successfully!');
        
        // Update the asset status locally
        this.assets.update(list => 
          list.map(a => a.id === dto.assetId ? { ...a, status: dto.status } : a)
        );
        
        this.calculateStats();
        this.closeReportIssueModal();
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to report issue.');
        this.isLoading.set(false);
      }
    });
  }

  openResolveModal(asset: AssetResponseDTO) {
    this.selectedAssetNameForResolve.set(asset.assetname);
    this.resolveForm.set({
      assetId: asset.id,
      status: 'active',
      report: ''
    });
    this.showResolveModal.set(true);
  }

  closeResolveModal() {
    this.showResolveModal.set(false);
  }

  submitResolve() {
    const dto = this.resolveForm();
    if (!dto.report?.trim()) {
      this.toastService.warning('Please provide a resolution note.');
      return;
    }

    this.isLoading.set(true);
    this.assetService.reportAssetIssue(dto).subscribe({
      next: (result) => {
        this.toastService.success('Asset status updated & issue addressed!');
        
        // Update asset locally
        this.assets.update(list => 
          list.map(a => a.id === dto.assetId ? { ...a, status: dto.status } : a)
        );
        
        this.calculateStats();
        this.loadReports();
        this.closeResolveModal();
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.toastService.error('Failed to update asset status.');
        this.isLoading.set(false);
      }
    });
  }

  getAssetNameById(assetId: number): string {
    const found = this.assets().find(a => a.id === assetId);
    return found ? found.assetname : `Asset #${assetId}`;
  }
}
