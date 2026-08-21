const targets=document.querySelectorAll('.section-heading,.feature-list article,.loop-copy,.loop-track li,.about>*');
targets.forEach(el=>el.classList.add('reveal'));
const observer=new IntersectionObserver(entries=>entries.forEach(entry=>{if(entry.isIntersecting){entry.target.classList.add('visible');observer.unobserve(entry.target)}}),{threshold:.14});
targets.forEach(el=>observer.observe(el));
