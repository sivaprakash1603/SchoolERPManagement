import { Component, HostListener, signal, computed, AfterViewInit, ElementRef, inject, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ThemeService } from '../../services/theme';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing implements AfterViewInit, OnDestroy {
  scrollY = signal(0);
  viewportH = signal(window.innerHeight);
  themeService = inject(ThemeService);
  private el = inject(ElementRef);
  private ticking = false;
  private observer!: IntersectionObserver;

  // ─── Scroll Progress Helpers (0 → 1 within a pixel band) ───
  /**
   * Returns a 0→1 value representing how far scrollY is between `start` and `end`.
   * Clamped at both ends.
   */
  progress(start: number, end: number): number {
    const s = this.scrollY();
    if (s <= start) return 0;
    if (s >= end) return 1;
    return (s - start) / (end - start);
  }

  // Eased progress with cubic ease-out
  eased(start: number, end: number): number {
    const t = this.progress(start, end);
    return 1 - Math.pow(1 - t, 3);
  }

  // Linear interpolation helper
  lerp(a: number, b: number, t: number): number {
    return a + (b - a) * t;
  }

  // ─── Hero Section Transforms ───
  heroOpacity = computed(() => {
    const t = this.progress(0, this.viewportH() * 0.8);
    return 1 - t;
  });

  heroScale = computed(() => {
    const t = this.progress(0, this.viewportH() * 0.8);
    return 1 - t * 0.15;
  });

  heroTranslateY = computed(() => {
    return this.scrollY() * 0.35;
  });

  // Mockup: tilt based on scroll
  mockupRotateX = computed(() => {
    const t = this.progress(0, this.viewportH() * 0.6);
    return this.lerp(8, 0, t);
  });

  mockupRotateY = computed(() => {
    const t = this.progress(0, this.viewportH() * 0.6);
    return this.lerp(-2, 0, t);
  });

  mockupScale = computed(() => {
    const t = this.progress(100, this.viewportH() * 0.5);
    return this.lerp(0.92, 1, t);
  });

  // Floating widget bounce
  widget1Y = computed(() => {
    return Math.sin(this.scrollY() * 0.008) * 12 + this.scrollY() * -0.15;
  });

  widget2Y = computed(() => {
    return Math.cos(this.scrollY() * 0.006) * 10 + this.scrollY() * -0.22;
  });

  widget1Rotate = computed(() => {
    return Math.sin(this.scrollY() * 0.005) * 3;
  });

  widget2Rotate = computed(() => {
    return Math.cos(this.scrollY() * 0.004) * -2.5;
  });

  // ─── Features Section Transforms ───
  featuresTitleScale = computed(() => {
    const vh = this.viewportH();
    const t = this.eased(vh * 0.5, vh * 1.2);
    return this.lerp(0.85, 1, t);
  });

  featuresTitleOpacity = computed(() => {
    const vh = this.viewportH();
    return this.eased(vh * 0.5, vh * 1.0);
  });

  // Individual feature card transforms (staggered rotation + translation)
  featureCard1 = computed(() => {
    const vh = this.viewportH();
    const t = this.eased(vh * 0.7, vh * 1.4);
    return {
      y: this.lerp(80, 0, t),
      rotate: this.lerp(-6, 0, t),
      scale: this.lerp(0.88, 1, t),
      opacity: t,
    };
  });

  featureCard2 = computed(() => {
    const vh = this.viewportH();
    const t = this.eased(vh * 0.85, vh * 1.5);
    return {
      y: this.lerp(100, 0, t),
      rotate: this.lerp(0, 0, t),
      scale: this.lerp(0.85, 1, t),
      opacity: t,
    };
  });

  featureCard3 = computed(() => {
    const vh = this.viewportH();
    const t = this.eased(vh * 0.95, vh * 1.6);
    return {
      y: this.lerp(80, 0, t),
      rotate: this.lerp(6, 0, t),
      scale: this.lerp(0.88, 1, t),
      opacity: t,
    };
  });

  // ─── Stats / Metrics Transforms ───
  statsProgress = computed(() => {
    const vh = this.viewportH();
    return this.eased(vh * 1.3, vh * 2.0);
  });

  metricCard(index: number) {
    const vh = this.viewportH();
    const stagger = index * 0.12;
    const start = vh * (1.5 + stagger);
    const end = vh * (2.0 + stagger);
    const t = this.eased(start, end);
    const rotations = [-4, 3, -5, 4];
    return {
      y: this.lerp(60, 0, t),
      rotate: this.lerp(rotations[index] || 0, 0, t),
      scale: this.lerp(0.8, 1, t),
      opacity: t,
    };
  }

  // ─── CTA Section Transforms ───
  ctaProgress = computed(() => {
    const vh = this.viewportH();
    return this.eased(vh * 2.0, vh * 2.6);
  });

  ctaScale = computed(() => {
    return this.lerp(0.9, 1, this.ctaProgress());
  });

  ctaRotateX = computed(() => {
    return this.lerp(8, 0, this.ctaProgress());
  });

  // ─── Parallax orbs ───
  orb(speed: number, phase: number) {
    const s = this.scrollY();
    return {
      y: s * speed,
      rotate: Math.sin(s * 0.003 + phase) * 15,
      scale: 1 + Math.sin(s * 0.002 + phase) * 0.1,
    };
  }

  // ─── Grid fade on scroll ───
  gridOpacity = computed(() => {
    return Math.max(0, 1 - this.progress(0, this.viewportH() * 0.5) * 0.8);
  });

  @HostListener('window:scroll', [])
  onWindowScroll() {
    if (!this.ticking) {
      requestAnimationFrame(() => {
        this.scrollY.set(window.scrollY || document.documentElement.scrollTop);
        this.ticking = false;
      });
      this.ticking = true;
    }
  }

  @HostListener('window:resize', [])
  onWindowResize() {
    this.viewportH.set(window.innerHeight);
  }

  ngAfterViewInit() {
    // Reveal observer with staggered thresholds
    const observerOptions = {
      root: null,
      rootMargin: '0px 0px -50px 0px',
      threshold: [0, 0.1, 0.2],
    };

    this.observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting && entry.intersectionRatio >= 0.1) {
          entry.target.classList.add('revealed');
          this.observer.unobserve(entry.target);
        }
      });
    }, observerOptions);

    const revealItems = this.el.nativeElement.querySelectorAll('.reveal-item');
    revealItems.forEach((item: HTMLElement) => {
      this.observer.observe(item);
    });
  }

  ngOnDestroy() {
    if (this.observer) {
      this.observer.disconnect();
    }
  }
}
